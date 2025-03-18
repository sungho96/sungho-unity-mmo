using RosMessageTypes.Std;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class SphereMover : MonoBehaviour
{
    public ROSConnection ros;
    public GameObject cube; // Unity 에디터에서 할당할 객체

    void Start()
    {
        // ROS 연결 인스턴스를 가져오거나 생성
        ros = ROSConnection.GetOrCreateInstance();

        // 구독 설정
        ros.Subscribe<StringMsg>("/current_pose", CoordinatesCallback);
    }

    void CoordinatesCallback(StringMsg msg)
    {
        string[] dataPairs = msg.data.Split(';');
        if (dataPairs.Length > 0)
        {
            string[] values = dataPairs[0].Trim(new char[] { '(', ')' }).Split(',');
            if (values.Length == 6)
            {
                float posX = -1 * float.Parse(values[1]) / 1000.0f;
                float posY = float.Parse(values[2]) / 1000.0f + 2f;
                float posZ = float.Parse(values[0]) / 1000.0f - 1.25f;
                float rotX = float.Parse(values[3]);
                float rotY = float.Parse(values[4]);
                float rotZ = float.Parse(values[5]);

                // 디버그 메시지 출력
                Debug.Log($"Received coordinates: posX={posX}, posY={posY}, posZ={posZ}, rotX={rotX}, rotY={rotY}, rotZ={rotZ}");

                // 기존 Sphere 위치 및 회전 업데이트
                Vector3 newPosition = new Vector3(posX, posY, posZ);
                Quaternion newRotation = Quaternion.Euler(rotX, rotY, rotZ);

                // 실시간으로 Sphere 이동
                cube.transform.position = newPosition;
                cube.transform.rotation = newRotation;
            }
            else
            {
                Debug.Log("Invalid number of values received.");
            }
        }
        else
        {
            Debug.Log("No data pairs received.");
        }
    }
}

