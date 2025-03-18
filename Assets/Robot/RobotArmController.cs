using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmController : MonoBehaviour
{
    public Transform[] joints; // 모든 관절의 Transform 배열
    public Transform endEffector; // 엔드 이펙터 Transform
    public GameObject robotBase; // 로봇 팔의 베이스

    // 이 메소드는 로봇 팔의 위치를 업데이트하는 데 사용됩니다.
    public void UpdateRobotArm(string data)
    {
        // 받은 데이터 파싱
        string[] parts = data.Split(';');
        string[] ikCoords = parts[0].Trim('(', ')').Split(',');
        string[] jointAngles = parts[1].Trim('(', ')').Split(',');

        // IK 좌표를 Vector3로 변환
        Vector3 targetPosition = new Vector3(
            float.Parse(ikCoords[0]), 
            float.Parse(ikCoords[1]), 
            float.Parse(ikCoords[2])
        );

        // 관절 각도를 float 배열로 변환
        float[] angles = new float[jointAngles.Length];
        for (int i = 0; i < jointAngles.Length; i++)
        {
            angles[i] = float.Parse(jointAngles[i]);
        }

        // IK를 사용하여 엔드 이펙터를 목표 위치로 이동
        MoveToTargetPosition(targetPosition);

        // 각 관절에 각도 적용
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].localRotation = Quaternion.Euler(0, angles[i], 0);
        }
    }

    // CCD 알고리즘을 사용하여 엔드 이펙터를 목표 위치로 이동
    private void MoveToTargetPosition(Vector3 targetPosition)
    {
        int maxIterations = 10;
        float threshold = 0.01f;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            for (int i = joints.Length - 1; i >= 0; i--)
            {
                Transform joint = joints[i];
                Vector3 toTarget = targetPosition - joint.position;
                Vector3 toEndEffector = endEffector.position - joint.position;

                float angle = Vector3.SignedAngle(toEndEffector, toTarget, Vector3.up);
                joint.Rotate(Vector3.up, angle, Space.World);

                if (Vector3.Distance(endEffector.position, targetPosition) < threshold)
                    return;
            }
        }
    }
}
