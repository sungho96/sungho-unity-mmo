using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

public class GT4_rail_test : MonoBehaviour
{
    public ROSConnection ros;
    public GameObject RobotArm;

    // 목표 위치로 이동할 z축 값을 저장할 변수
    private float targetPositionZ = 0.31f;  // 초기 z축 위치
    private float previousTargetPositionZ = 0.31f;  // 이전 z축 위치를 저장하는 변수

    // 이동 속도 조절 변수
    public float moveSpeed = 2.0f;  // 값을 높이면 더 빨리 이동, 낮추면 천천히 이동

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>("/rcs/rail_actpos", RobotRailCallback);
    }

    void Update()
    {
        // 현재 로봇 팔의 위치 가져오기
        Vector3 currentPosition = RobotArm.transform.position;

        // 목표 위치와 현재 위치가 다를 때만 업데이트
        if (Mathf.Abs(currentPosition.z - targetPositionZ) > 0.0001f)  // 약간의 오차 허용
        {
            // z 값만 목표 위치로 설정
            currentPosition.z = Mathf.Lerp(currentPosition.z, targetPositionZ, moveSpeed * Time.deltaTime);

            // 절대 좌표계에서 위치 설정
            RobotArm.transform.position = currentPosition;

            // 디버그 로그로 로봇의 현재 z축 위치 출력
           // Debug.Log($"Moving RobotArm to global z position: {currentPosition.z}");
        }
    }

    void RobotRailCallback(StringMsg msg)
    {
        // ROS로부터 수신된 데이터 파싱
        if (float.TryParse(msg.data, out float value))
        {
            // 파싱된 값을 100으로 나눈 후 목표 z 위치로 설정
            float newTargetPositionZ = value +0.31f;

            // 새로운 목표 위치와 이전 목표 위치가 다를 때만 업데이트
            if (Mathf.Abs(newTargetPositionZ - previousTargetPositionZ) > 0.01f)
            {
                targetPositionZ = newTargetPositionZ;
                previousTargetPositionZ = newTargetPositionZ;

                // 디버그 로그 출력
                Debug.Log($"Received rail value: {value}, After division: {targetPositionZ}");
            }
        }
        else
        {
            Debug.LogWarning("레일 값을 파싱하는 데 실패했습니다: " + msg.data);
        }
    }
}
