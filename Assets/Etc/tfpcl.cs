using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;
using RosMessageTypes.Std;

public class AdvancedPointCloudVisualizer : MonoBehaviour
{
    public ROSConnection ros;
    private GameObject[] pointCloudObjects;
    private GameObject pointCloudParent;
    private const int POINTS_LIMIT = 10000;
    private const float Rad2Deg = 57.2958f; // 라디안에서 디그리로 변환하는 상수
    private bool holdLastPoints = false; // 마지막 포인트들을 hold할지 여부를 결정하는 플래그

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PointCloud2Msg>("/pointcloud_topic", PointCloudCallback);
        ros.Subscribe<StringMsg>("/transform_camera", StringMessageCallback);
        ros.Subscribe<StringMsg>("/robot_pose", PoseCallback); // 새로운 ROS 토픽 구독

        pointCloudParent = new GameObject("PointCloudParent");

        pointCloudObjects = new GameObject[POINTS_LIMIT];
        for (int i = 0; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointCloudObjects[i].transform.localScale = new Vector3(0.025f, 0.01f, 0.025f);
            pointCloudObjects[i].SetActive(false);
            pointCloudObjects[i].transform.parent = pointCloudParent.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            holdLastPoints = !holdLastPoints;
            if (holdLastPoints)
            {
                Debug.Log("Point cloud data reception paused.");
                DisableAllPoints_1();
            }
            else
            {
                Debug.Log("Point cloud data reception resumed.");
            }
        }
    }

    void PointCloudCallback(PointCloud2Msg msg)
    {
        if (holdLastPoints)
        {
            // holdLastPoints가 true인 경우 새로운 포인트를 생성하지 않음
            return;
        }

        int totalPoints = Math.Min((int)msg.width * (int)msg.height, POINTS_LIMIT);

        for (int i = 0; i < totalPoints; i++)
        {
            int startIndex = i * (int)msg.point_step;
            if (startIndex + 12 >= msg.data.Length)
            {
                Debug.LogError($"Index out of range: startIndex {startIndex} exceeds data length {msg.data.Length}.");
                break;
            }

            float x = BitConverter.ToSingle(msg.data, startIndex);
            float y = BitConverter.ToSingle(msg.data, startIndex + 4);
            float z = BitConverter.ToSingle(msg.data, startIndex + 8);
            uint color = BitConverter.ToUInt32(msg.data, startIndex + 12);
            z = -z;

            // 색상 분리
            Color32 pointColor = new Color32(
                (byte)((color >> 16) & 0xFF), // Red
                (byte)((color >> 8) & 0xFF),  // Green
                (byte)(color & 0xFF),         // Blue
                (byte)((color >> 24) & 0xFF)  // Alpha
            );

            Vector3 pointPosition = new Vector3(x, y, z);
            pointCloudObjects[i].SetActive(true);
            pointCloudObjects[i].transform.localPosition = pointPosition;
            pointCloudObjects[i].GetComponent<Renderer>().material.color = pointColor;
        }

        for (int i = totalPoints; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i].SetActive(false);
        }
    }

    void StringMessageCallback(StringMsg msg)
    {
        string[] parts = msg.data.Trim(new char[] { '[', ']' }).Split(',');

        if (parts.Length != 6)
        {
            Debug.LogError("Invalid message format. Expected 6 numbers.");
            return;
        }

        try
        {
            Vector3 pos = new Vector3(
                float.Parse(parts[0]),
                float.Parse(parts[2]),
                float.Parse(parts[1])
            );

            Vector3 eulerAngles = new Vector3(
                float.Parse(parts[4]) * Rad2Deg + 90.0f, // 라디안에서 디그리로 변환 후 90도 추가
                float.Parse(parts[5]) * -Rad2Deg, // 라디안에서 디그리로 변환
                float.Parse(parts[3]) * Rad2Deg 
            );
            //Vector3 base_angels = new Vector3(90.0f,0.0f,0.0f);

            //pointCloudParent.transform.eulerAngles = base_angels;
            pointCloudParent.transform.eulerAngles = eulerAngles;
            pointCloudParent.transform.position = pos;

            //Debug.Log("Applied Rotation (Euler Angles): " + pointCloudParent.transform.eulerAngles);
            //Debug.Log("Applied Position: " + pointCloudParent.transform.position);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation: " + ex.Message);
        }
    }

    void PoseCallback(StringMsg message)
    {
        string data = message.data;
        string[] stringValues = data.Split(';');

        if (stringValues.Length < 1)
        {
            Debug.LogError("Invalid robot_pose message format.");
            return;
        }

        string poseType = stringValues[0].Trim();

        if (poseType != "stop")
        {
            // "hold" 기능 수행
            holdLastPoints = true;
            //Debug.Log("Holding the last points based on /robot_pose message.");
        }
        else 
        {
            // "unhold" 기능 수행
            holdLastPoints = false; 
            //Debug.Log("Unholding the last points based on");
            //Invoke("UnholdPoints", 15.0f);
        }
    }

    void DisableAllPoints_1()
    {
        for (int i = 0; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i].SetActive(false);
        }
        Debug.Log("All points have been disabled.");
    }

    void UnholdPoints()
    {
        holdLastPoints = false;

        // 기존 포인트들 모두 삭제
        for (int i = 0; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i].SetActive(false);
        }
        Debug.Log("Unholding the last points after delay.");
    }
}
