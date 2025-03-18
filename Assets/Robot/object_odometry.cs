using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 버튼을 사용하기 위해 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System;

public class ObjectOdometry : MonoBehaviour
{
    public ROSConnection ros;
    public GameObject Robot;
    public Button ToggleButton; // 버튼 연결

  //  private bool isProcessingEnabled = true;  
  //  private bool isInitialValueSet = false;   
    private bool isResetActive = false; // 리셋 상태를 저장

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>("/fastlio_odom", FastLioOdomCallback);

        // 버튼 클릭 이벤트 연결
        if (ToggleButton != null)
        {
            ToggleButton.onClick.AddListener(OnToggleButtonClicked);
        }
    }
    void Update()
    {
        if (isResetActive)
        {
            // 리셋 상태일 때 로봇의 위치와 회전을 강제로 고정
            Robot.transform.position = Vector3.zero;
            Robot.transform.eulerAngles = Vector3.zero;
            return; // 다른 업데이트를 막음
        }
    }
    void OnToggleButtonClicked()
    {
        isResetActive = !isResetActive;
        ResetStateEvent.BroadcastResetState(isResetActive);

        if (isResetActive)
        {
            Debug.Log("[ObjectOdometry] Reset mode activated.");
            Robot.transform.position = Vector3.zero;
            Robot.transform.eulerAngles = Vector3.zero;
        }
        else
        {
            Debug.Log("[ObjectOdometry] Reset mode deactivated.");
        }
    }


    void FastLioOdomCallback(StringMsg message)
    {
        if (isResetActive)
        {
            // 리셋 모드일 경우 메시지 처리를 무시
            return;
        }
        string[] parts = message.data.Split(';');
        if (parts.Length != 10)
        {
            Debug.LogError("Invalid message format for /fastlio_odom. Expected 10 values.");
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
            
            float x = float.Parse(parts[1]) * -1;
            float y = 0;
            float z = float.Parse(parts[0]);

            Vector3 pos = new Vector3(x, y, z);
            Robot.transform.position = pos;
            Robot.transform.eulerAngles = eulerAngles;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }

    }
}
