using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;

public class DualLidarSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string frontLidarTopic = "/front_scan";
    public string rearLidarTopic = "/rear_scan";

    public List<Vector3> frontLidarPoints = new List<Vector3>();
    public List<Vector3> rearLidarPoints = new List<Vector3>();

    private List<GameObject> frontLidarCubes = new List<GameObject>();
    private List<GameObject> rearLidarCubes = new List<GameObject>();

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(frontLidarTopic, ProcessFrontLidarData);
        ros.Subscribe<LaserScanMsg>(rearLidarTopic, ProcessRearLidarData);
    }

    void ProcessFrontLidarData(LaserScanMsg msg)
    {
        frontLidarPoints.Clear();
        float angle = msg.angle_min;
        for (int i = 0; i < msg.ranges.Length; i++)
        {
            float range = msg.ranges[i];
            if (range < msg.range_min || range > msg.range_max)
            {
                angle += msg.angle_increment;
                continue;
            }
            float x = range * Mathf.Cos(angle);
            float y = range * Mathf.Sin(angle);
            frontLidarPoints.Add(new Vector3(x, 0, y));
            angle += msg.angle_increment;
        }

        UpdateLidarVisualization(frontLidarPoints, frontLidarCubes);
    }

    void ProcessRearLidarData(LaserScanMsg msg)
    {
        rearLidarPoints.Clear();
        float angle = msg.angle_min;
        for (int i = 0; i < msg.ranges.Length; i++)
        {
            float range = msg.ranges[i];
            if (range < msg.range_min || range > msg.range_max)
            {
                angle += msg.angle_increment;
                continue;
            }
            float x = range * Mathf.Cos(angle);
            float y = range * Mathf.Sin(angle);
            rearLidarPoints.Add(new Vector3(x, 0, y));
            angle += msg.angle_increment;
        }

        UpdateLidarVisualization(rearLidarPoints, rearLidarCubes);
    }

    void UpdateLidarVisualization(List<Vector3> lidarPoints, List<GameObject> lidarCubes)
    {
        // 기존 큐브 삭제
        foreach (var cube in lidarCubes)
        {
            Destroy(cube);
        }
        lidarCubes.Clear();

        // 새 큐브 생성
        foreach (var point in lidarPoints)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = point;
            cube.transform.localScale = Vector3.one * 0.1f; // 큐브 크기 조절
            lidarCubes.Add(cube);
        }
    }
}
