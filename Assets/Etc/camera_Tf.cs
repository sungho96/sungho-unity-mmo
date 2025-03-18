using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System;

public class Camera_TF : MonoBehaviour
{
    public ROSConnection ros;
    public Camera mainCamera;
    public Vector3 storedPosition;
    public Vector3 storedEulerAngles;
    
    private const float Deg2Rad = 0.0174533f;

    // positionOffset 값을 0.26, 1.58, -0.71로 설정
    private Vector3 positionOffset = new Vector3(0.26f, 1.58f, -0.71f);
    private Vector3 eulerOffset = new Vector3(0.0f, 0.0f, 0.0f); // 필요 없으면 0으로 둠

    public Vector3 camera_tf_pos = new Vector3();
    public Vector3 camera_tf_eul = new Vector3();
    private bool isProcessingEnabled = false;  
    private bool isInitialValueSet = false;   

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROSConnection component not found on the GameObject.");
            return;
        }
      
        ros.Subscribe<StringMsg>("/fastlio_odom", FastLioOdomCallback);
        ros.Subscribe<StringMsg>("/unity/cmd", DirectionCallback);
    }
    void FastLioOdomCallback(StringMsg message)
    {
        if (!isInitialValueSet)
        {
            ProcessOdomMessage(message);
            isInitialValueSet = true;
            return;
        }

        if (isProcessingEnabled)
        {
            ProcessOdomMessage(message);
        }
    }
     void ProcessOdomMessage(StringMsg message)
    {
        string[] parts = message.data.Split(';');

        if (parts.Length != 10)
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
            eulerAngles.y *= -1;
            eulerAngles.x = 0;

            // 오프셋 적용
            eulerAngles += eulerOffset;

            float x = float.Parse(parts[1]) * -1;
            float y = 0;
            float z = float.Parse(parts[0]);

            Vector3 pos = new Vector3(x, y, z);

            storedPosition = pos;
            storedEulerAngles = eulerAngles;

            // 회전 각도 계산 후 좌표 변환
            float storedRadian = storedEulerAngles.y * Deg2Rad * -1;

            Vector3 point = positionOffset; // 예시로 point 벡터 설정
            float x_2 = point.x * Mathf.Cos(storedRadian) - point.z * Mathf.Sin(storedRadian);
            float z_2 = point.x * Mathf.Sin(storedRadian) + point.z * Mathf.Cos(storedRadian);

            // 변환된 좌표에 storedPosition 더하기
            Vector3 pointPosition = new Vector3(x_2, point.y, z_2) + storedPosition;

            // 카메라의 위치와 회전값을 적용
            camera_tf_pos = pointPosition;
            camera_tf_eul = Quaternion.Euler(storedEulerAngles).eulerAngles;
            //Debug.Log($"pos: {mainCamera.transform.position}, rot : {mainCamera.transform.rotation}");  


        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }
    }  
    void DirectionCallback(StringMsg message)
    {
        if (message.data == "start")
        {
           
            isProcessingEnabled = true;
        }
        else if (message.data == "completed")
        {
           
            isProcessingEnabled = false;
        }
    }
}
