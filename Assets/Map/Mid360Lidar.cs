using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;
using System.Collections.Generic;
using System.Threading;

public class LidarVisualizer : MonoBehaviour
{
    ROSConnection ros;
    public string lidarTopic = "/mid360_point";
    public GameObject pointPrefab;
    private GameObject lidarParent;

    private List<GameObject> points = new List<GameObject>();
    public int maxPoints = 5000; // 최대 포인트 개수
    public int currentPointCount = 0; // 현재 포인트 개수 확인용
    public float positionThreshold = 0.1f; // 위치 비교 시 허용 오차 범위

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>(lidarTopic, VisualizeLidarData);

        // 부모 객체 생성
        lidarParent = new GameObject("LidarParent");
    }

    void VisualizeLidarData(StringMsg msg)
    {
        
        string[] pointStrings = msg.data.Split(';');
        foreach (string pointString in pointStrings)
        {
            if (string.IsNullOrWhiteSpace(pointString)) continue;

            string[] coords = pointString.Trim('(', ')').Split(',');
            if (coords.Length == 3)
            {
                float x = float.Parse(coords[1]) * -1;
                float y = float.Parse(coords[2]) + 1;
                float z = float.Parse(coords[0]) - 0.25f;

                Vector3 pointPosition = new Vector3(x, y, z);

                // 기존 포인트와 위치 비교하여 가까운 포인트가 있으면 해당 포인트를 이동
                bool foundSimilarPoint = false;
                foreach (var point in points)
                {
                    if (Vector3.Distance(point.transform.position, pointPosition) < positionThreshold)
                    {
                        point.transform.position = Vector3.Lerp(point.transform.position, pointPosition, 0.5f);
                        foundSimilarPoint = true;
                        break;
                    }
                }

                // 비슷한 위치의 포인트가 없으면 새로운 포인트 생성
                if (!foundSimilarPoint)
                {
                    if (points.Count >= maxPoints)
                    {
                        GameObject oldPoint = points[0];
                        points.RemoveAt(0);
                        Destroy(oldPoint);
                    }

                    GameObject pointObject = Instantiate(pointPrefab, pointPosition, Quaternion.identity, lidarParent.transform);
                    points.Add(pointObject);
                    currentPointCount = points.Count; // 현재 포인트 개수 업데이트
                    //Debug.Log($"Count:{currentPointCount}");
                }
            }
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
            Quaternion rot = new Quaternion(
                float.Parse(parts[4]) * -1,
                float.Parse(parts[5]),
                float.Parse(parts[3]),
                float.Parse(parts[6])
            );

            Vector3 eulerAngles = rot.eulerAngles;
            float rad_eulerY = eulerAngles.y * Mathf.PI / 180 * -1;

            float x = float.Parse(parts[1]) - Mathf.Sin(rad_eulerY) * 0.25f;
            float y = float.Parse(parts[2]) * -1;
            float z = (float.Parse(parts[0]) + Mathf.Cos(rad_eulerY) * 0.25f - 0.25f) * -1;

            float x_1 = x * Mathf.Cos(rad_eulerY) - z * Mathf.Sin(rad_eulerY);
            float z_1 = x * Mathf.Sin(rad_eulerY) + z * Mathf.Cos(rad_eulerY);

            Vector3 pos = new Vector3(x_1, y, z_1);
            lidarParent.transform.position = pos;
            lidarParent.transform.eulerAngles = eulerAngles;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }
    }
}
