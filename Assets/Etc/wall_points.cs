using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

public class WallSpawner : MonoBehaviour
{
    public ROSConnection ros; // ROS 연결을 위한 변수
    public GameObject wallPrefab; // 벽 프리팹을 참조할 변수
    private GameObject wallParent; // 부모 객체
    private Dictionary<string, GameObject> existingWalls = new Dictionary<string, GameObject>(); // 생성된 벽의 위치를 추적하기 위한 딕셔너리

    void Start()
    {
        // ROS 연결 설정
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROSConnection component not found on the GameObject.");
            return;
        }

        // ROS 구독 설정
        ros.Subscribe<StringMsg>("wall_points", WallCallback);

        // 부모 객체 생성
        wallParent = new GameObject("WallParent");

        // 부모 객체의 위치 보정
        wallParent.transform.position = new Vector3(0, 0, 0);
    }

    void WallCallback(StringMsg message)
    {
        // 메시지 데이터 파싱
        string data = message.data.Trim(new char[] { '[', ']' });
        string[] pointPairs = data.Split(new string[] { "), (" }, StringSplitOptions.RemoveEmptyEntries);

        if (pointPairs.Length % 2 != 0)
        {
            Debug.LogError("Invalid point data received.");
            return;
        }

        for (int i = 0; i < pointPairs.Length; i += 2)
        {
            string[] point1 = pointPairs[i].Trim(new char[] { '(', ')' }).Split(',');
            string[] point2 = pointPairs[i + 1].Trim(new char[] { '(', ')' }).Split(',');

            if (point1.Length != 2 || point2.Length != 2)
            {
                Debug.LogError("Invalid point format.");
                return;
            }

            Vector3 startPosition = new Vector3(float.Parse(point1[0]), 0, float.Parse(point1[1]));
            Vector3 endPosition = new Vector3(float.Parse(point2[0]), 0, float.Parse(point2[1]));

            CreateWall(startPosition, endPosition);
        }
    }

    void CreateWall(Vector3 startPosition, Vector3 endPosition)
    {
        if (wallPrefab == null)
        {
            Debug.LogError("Wall prefab is not assigned.");
            return;
        }

        float width = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), new Vector3(endPosition.x, 0, endPosition.z));
        float height = 2.5f; // 벽 높이 고정

        Vector3 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        // 벽의 크기와 위치 설정
        Vector3 scale = new Vector3(width, height, 0.001f); // 두께는 0.1m로 설정
        Vector3 spawnPosition = (startPosition + endPosition) / 2 + new Vector3(0, height / 2, 0); // 벽의 중심이 아니라 바닥에서 생성되도록 위치 조정

        // 벽 위치 키 생성
        string wallKey = $"{startPosition.x},{startPosition.z}:{endPosition.x},{endPosition.z}";

        // 이미 존재하는 위치에 벽이 있는지 확인
        if (existingWalls.ContainsKey(wallKey))
        {
            // 기존 벽 재활용
            GameObject existingWall = existingWalls[wallKey];
            existingWall.transform.position = spawnPosition;
            existingWall.transform.localScale = scale;
            //existingWall.transform.rotation = Quaternion.Euler(0, -angle, 0);
        }
        else
        {
            // 새로운 벽 생성
            GameObject wall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity);
            wall.transform.localScale = scale;
            wall.transform.SetParent(wallParent.transform);
            wall.transform.rotation = Quaternion.Euler(0, -angle, 0);

            // 생성된 벽을 딕셔너리에 추가
            existingWalls[wallKey] = wall;

            // 벽 삭제 코루틴 시작
            StartCoroutine(DestroyWallAfterDelay(wall, 0.5f));
        }
    }

    IEnumerator DestroyWallAfterDelay(GameObject wall, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (wall != null)
        {
            existingWalls.Remove(wall.GetInstanceID().ToString());
            Destroy(wall);
        }
    }
}
