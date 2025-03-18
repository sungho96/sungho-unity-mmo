using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Sensor;
using RosMessageTypes.Tf2;
using System;
using Robotics;
using RosMessageTypes.Std;

public class IKManager : MonoBehaviour
{
    public customJoint m_root;
    public customJoint m_end;
    public GameObject m_target;
    public float m_threshold = 0.1f;
    public float m_rate = 5.0f;
    public int m_steps = 1;
    public float[] current_angles = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }; // 현재 관절 각도 초기화
    public enum CubeFace { Front, Back, Left, Right, Top, Bottom }
    public CubeFace targetFace;

    private ROSConnection ros;
    private List<customJoint> joints = new List<customJoint>();

    void Start()
    {
        InitializeEndEffectorOrientation();

        // ROS 연결 초기화
        ros = ROSConnection.GetOrCreateInstance();
        // /current_pose 토픽을 구독하여 UpdateRobotArm 메서드를 호출
        ros.Subscribe<StringMsg>("/current_pose", UpdateRobotArm);

        // joints 리스트 초기화
        Transform current = m_root.transform;
        while (current != null)
        {
            joints.Add(current.GetComponent<customJoint>());
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }
        m_target.SetActive(false);
    }

    void UpdateRobotArm(StringMsg message)
    {
        // ROS 메시지에서 데이터를 추출
        string data = message.data;

        // 받은 데이터 파싱
        string[] parts = data.Split(';');
        if (parts.Length < 2)
        {
            Debug.LogError("Invalid message format.");
            return;
        }

        string[] ikCoords = parts[0].Trim('(', ')').Split(',');
        string[] jointAngles = parts[1].Trim('(', ')').Split(',');

        if (ikCoords.Length < 3)
        {
            Debug.LogError("Invalid IK coordinates or joint angles.");
            return;
        }

        // IK 좌표를 Vector3로 변환
        Vector3 targetPosition = new Vector3(
            float.Parse(ikCoords[0]), 
            float.Parse(ikCoords[1]), 
            float.Parse(ikCoords[2])
        );

        //Debug.Log($"Target position: {targetPosition}");

        // IK 좌표를 목표로 설정
        SetTargetPosition(targetPosition);

        // 관절 각도를 float 배열로 변환
        float[] angles = new float[jointAngles.Length];
        float[] rotate_angles = new float[jointAngles.Length];
        
        for (int i = 0; i < jointAngles.Length; i++)
        {
            if (i ==0 || i ==2 || i==4)
            {
            angles[i] = -float.Parse(jointAngles[i]);
            }
            else
            {
            angles[i] = float.Parse(jointAngles[i]);
            }
            
            rotate_angles[i] = angles[i] - current_angles[i];
            current_angles[i] = angles[i];
        }
        
        for (int i = 0; i < m_steps; ++i)
        {
            if (GetDistance(m_end.transform.position, GetTargetFacePosition()) > m_threshold)
            {
                Transform current = m_root.transform;
                int j = 0;
                
                while (current != null )
                {
              
                    float slope = CalculateSlope(current);
                    if (j < 6)
                    {
                        current.Rotate(new Vector3(0, rotate_angles[j], 0));
                        //Debug.Log($"Joint {j} angle: {angles[j]}");
                    }
                    current = current.childCount > 0 ? current.GetChild(0) : null;
                    
                    j += 1;
                                     
                    
                }
            }
            AdjustEndEffectorOrientation();
        }
    }

    void SetTargetPosition(Vector3 targetPosition)
    {
        m_target.transform.position = targetPosition;
    }

    float CalculateSlope(Transform jointTransform)
    {
        float deltaTheta = 0.01f;
        float distance1 = GetDistance(m_end.transform.position, GetTargetFacePosition());
        jointTransform.Rotate(new Vector3(0, deltaTheta, 0));
        float distance2 = GetDistance(m_end.transform.position, GetTargetFacePosition());
        jointTransform.Rotate(new Vector3(0, -deltaTheta, 0));
        return (distance2 - distance1) / deltaTheta;
    }

    Vector3 GetTargetFacePosition()
    {
        Vector3 targetPosition = m_target.transform.position;
        Vector3 targetScale = m_target.transform.localScale;
        Quaternion targetRotation = m_target.transform.rotation;
        switch (targetFace)
        {
            case CubeFace.Front: return targetPosition + targetRotation * new Vector3(0, 0, targetScale.z / 2);
            case CubeFace.Back: return targetPosition + targetRotation * new Vector3(0, 0, -targetScale.z / 2);
            case CubeFace.Left: return targetPosition + targetRotation * new Vector3(-targetScale.x / 2, 0, 0);
            case CubeFace.Right: return targetPosition + targetRotation * new Vector3(targetScale.x / 2, 0, 0);
            case CubeFace.Top: return targetPosition + targetRotation * new Vector3(0, targetScale.y / 2, 0);
            case CubeFace.Bottom: return targetPosition + targetRotation * new Vector3(0, -targetScale.y / 2, 0);
            default: return targetPosition;
        }
    }

    Vector3 GetTargetFaceNormal()
    {
        switch (targetFace)
        {
            case CubeFace.Front: return m_target.transform.forward;
            case CubeFace.Back: return -m_target.transform.forward;
            case CubeFace.Left: return -m_target.transform.right;
            case CubeFace.Right: return m_target.transform.right;
            case CubeFace.Top: return m_target.transform.up;
            case CubeFace.Bottom: return -m_target.transform.up;
            default: return m_target.transform.forward;
        }
    }

    void InitializeEndEffectorOrientation()
    {
        Vector3 targetNormal = GetTargetFaceNormal();
        Vector3 initialDirection = GetTargetFacePosition() - m_end.transform.position;
        m_end.transform.rotation = Quaternion.LookRotation(initialDirection, targetNormal);
    }

    void AdjustEndEffectorOrientation()
    {
        Vector3 targetPosition = GetTargetFacePosition();
        Vector3 targetNormal = GetTargetFaceNormal();
        Vector3 directionToTarget = (targetPosition - m_end.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget, targetNormal);
        m_end.transform.rotation = lookRotation;
    }

    float GetDistance(Vector3 point1, Vector3 point2)
    {
        return Vector3.Distance(point1, point2);
    }
}

