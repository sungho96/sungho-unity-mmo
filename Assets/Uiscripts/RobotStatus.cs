using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using System;

public class RobotStatus : MonoBehaviour
{
    private ROSConnection ros;
    public Check_UI_Manager uiManager;
    public Button Setting_Button;
    public Button Cancel;
    public GameObject Setting_Panel;
    
    public Button Rainbow_Start;

    private float poseTimeout = 0f;
    private float pc2Timeout = 0f;
    private float lidarTimeout = 0f;

    private const float timeoutLimit = 5f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        //ros.RegisterPublisher<StringMsg>("/robot_pose");
        Setting_Panel.SetActive(true);
        ros.Subscribe<StringMsg>("/current_pose", OnPoseReceived);
        ros.Subscribe<PointCloud2Msg>("/pointcloud_topic", OnPointCloudReceived);
        ros.Subscribe<StringMsg>("/fastlio_odom", OnLidarReceived);
        Rainbow_Start.onClick.AddListener(() => RobotStart());
        Setting_Button.onClick.AddListener(OnStartButtonClick);
        Cancel.onClick.AddListener(OnStopButtonClick);
    }

    void Update()
    {
        poseTimeout += Time.deltaTime;
        pc2Timeout += Time.deltaTime;
        lidarTimeout += Time.deltaTime;

        if(poseTimeout > timeoutLimit) uiManager.UpdatePoseStatus(false);
        if(pc2Timeout > timeoutLimit) uiManager.UpdatePc2Status(false);
        if(lidarTimeout > timeoutLimit) uiManager.UpdateLidarStatus(false);

    }
    void RobotStart()
    {
        StringMsg message = new StringMsg("rainbow_start");
        ros.Publish("/unity/cmd",message);
    }
    void OnStartButtonClick()
    {
        Setting_Panel.SetActive(true);
    }
    void OnStopButtonClick()
    {
        Setting_Panel.SetActive(false);
    }

    void OnPoseReceived(StringMsg msg)
    {
        poseTimeout = 0f;
        uiManager.UpdatePoseStatus(true);
    }
    void OnPointCloudReceived(PointCloud2Msg msg)
    {
        pc2Timeout = 0f;
        uiManager.UpdatePc2Status(true);
    }
    void OnLidarReceived(StringMsg msg)
    {
        lidarTimeout = 0f;
        uiManager.UpdateLidarStatus(true);
    }

}
