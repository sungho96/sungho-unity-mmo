using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Check_UI_Manager : MonoBehaviour
{   
    public static Check_UI_Manager Instance;

    public UnityEngine.UI.Image lidarStatusIcon;
    public UnityEngine.UI.Image MiniLidarStatusIcon;
    public Text lidarStatusText;
    
    public UnityEngine.UI.Image pc2StatusIcon;
    public UnityEngine.UI.Image MiniPc2StatusIcon;
    public Text pc2StatusText;

    public UnityEngine.UI.Image poseStatusIcon;
    public UnityEngine.UI.Image MiniPoseStatusIcon;
    public Text poseStatusText;

    public Toggle Robot;
    public GameObject Robot_GT;

    private void Awake()
    {
        if (Instance == null) Instance =this;
    }
     void Start()
     {
        Robot_GT.SetActive(true);
        Robot.isOn = true;
        Robot.onValueChanged.AddListener(RobotActive);
     }
    
    public void UpdateLidarStatus(bool isConnected)
    {
        lidarStatusIcon.color = isConnected ? Color.green : Color.red;
        MiniLidarStatusIcon.color = isConnected ? Color.green : Color.red;
        lidarStatusText.text = isConnected ? "Connected" : "Error";
    }
        public void UpdatePc2Status(bool isConnected)
    {
        pc2StatusIcon.color = isConnected ? Color.green : Color.red;
        MiniPc2StatusIcon.color = isConnected ? Color.green : Color.red;
        pc2StatusText.text = isConnected ? "Connected" : "Stop";
    }

    // 로봇 Pose 상태 업데이트
    public void UpdatePoseStatus(bool isConnected)
    {
        poseStatusIcon.color = isConnected ? Color.green : Color.red;
        MiniPoseStatusIcon.color = isConnected ? Color.green : Color.red;
        poseStatusText.text = isConnected ? "Connected" : "Error";
    }

    private void RobotActive(bool isOn)
    {
        // 토글 상태에 따라 Robot_GT 활성화/비활성화
        Robot_GT.SetActive(isOn);
    }
}
