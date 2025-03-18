using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;
using RosMessageTypes.Std;

public class willmake : MonoBehaviour
{
    public ROSConnection ros;
    private GameObject[] pointCloudObjects;
    private List<GameObject> clonedPointCloudObjects = new List<GameObject>();
    private List<GameObject> pointCloudParents = new List<GameObject>(); // 저장된 부모 객체 리스트
    private GameObject pointCloudParent;
    private GameObject fixedPointCloudParent; // 새로운 부모 객체
    private const int POINTS_LIMIT = 10000;
    private const float Rad2Deg = 57.2958f; // 라디안에서 디그리로 변환하는 상수
    private bool holdLastPoints = false; // 마지막 포인트들을 hold할지 여부를 결정하는 플래그

    // UI Button reference
    public Button holdButton;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PointCloud2Msg>("/pointcloud_topic", PointCloudCallback);
        ros.Subscribe<StringMsg>("/transform_camera", StringMessageCallback);
        ros.Subscribe<StringMsg>("/robot_pose", PoseCallback); // 새로운 ROS 토픽 구독
        ros.Subscribe<StringMsg>("/fastlio_odom", FastLioOdomCallback); // 새로운 ROS 토픽 구독

        pointCloudParent = new GameObject("PointCloudParent");
        fixedPointCloudParent = new GameObject("FixedPointCloudParent"); // 새로운 부모 객체 생성

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
                DisableAllPoints();
            }
            else
            {
                Debug.Log("Point cloud data reception resumed.");
            }
        }
        // C 키가 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClonePointCloud();
        }
        // O 키가 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.O))
        {
            DeleteLastPointCloud();
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

            pointCloudParent.transform.eulerAngles = eulerAngles;
            pointCloudParent.transform.position = pos;

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
            //Debug.Log("Unholding the last points based on /robot_pose message.");
        }
    }

    void FastLioOdomCallback(StringMsg message)
    {
        string[] parts = message.data.Split(';');

        if (parts.Length != 7)
        {
            Debug.LogError("Invalid message format for /fastlio_odom. Expected 7 values.");
            return;
        }

        try
        {   
            float x =  float.Parse(parts[1]);
            float y =  float.Parse(parts[2])*-1;
            float z =  float.Parse(parts[0])*-1;

            Quaternion rot = new Quaternion(
                float.Parse(parts[4])*-1,
                float.Parse(parts[5]),
                float.Parse(parts[3]),
                float.Parse(parts[6])
            );

            Vector3 eulerAngles =rot.eulerAngles;

            float eulerX = eulerAngles.x;
            float eulerY = eulerAngles.y;
            float eulerZ = eulerAngles.z;
            
            float rad_eulerY = eulerY * 3.14f/180 *-1 ;
            float x_1 = x * Mathf.Cos(rad_eulerY)- z * Mathf.Sin(rad_eulerY);
            float z_1 = x * Mathf.Sin(rad_eulerY) + z * Mathf.Cos(rad_eulerY);

            Vector3 pos = new Vector3(
                x_1, y, z_1
            );

            fixedPointCloudParent.transform.position = pos;
            fixedPointCloudParent.transform.eulerAngles = eulerAngles;
            //Debug.Log($"eulerX:{eulerX},eulerY{eulerY},eulerZ{eulerZ}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }
    }

    public void ClonePointCloud()
    {
        Debug.Log("C key pressed - Cloning Point Cloud");

        GameObject newParent = new GameObject("PointCloudCloneParent");
        newParent.transform.parent = fixedPointCloudParent.transform;
        pointCloudParents.Add(newParent);

        // 현재 포인트 클라우드 상태를 클론하여 고정합니다.
        for (int i = 0; i < POINTS_LIMIT; i++)
        {
            if (pointCloudObjects[i].activeSelf)
            {
                GameObject clone = Instantiate(pointCloudObjects[i], pointCloudObjects[i].transform.position, pointCloudObjects[i].transform.rotation);
                clone.transform.localScale = pointCloudObjects[i].transform.localScale;
                clone.GetComponent<Renderer>().material.color = pointCloudObjects[i].GetComponent<Renderer>().material.color;
                clone.transform.parent = newParent.transform; // 새로운 부모 객체에 추가
                clonedPointCloudObjects.Add(clone);
            }
        }
    }

    public void DeleteLastPointCloud()
    {
        if (pointCloudParents.Count > 0)
        {
            GameObject lastParent = pointCloudParents[pointCloudParents.Count - 1];
            pointCloudParents.RemoveAt(pointCloudParents.Count - 1);
            Destroy(lastParent);
            Debug.Log("Last cloned point cloud deleted.");
        }
        else
        {
            Debug.LogWarning("No cloned point clouds to delete.");
        }
    }

    void DisableAllPoints()
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
