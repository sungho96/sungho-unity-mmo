using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using RosMessageTypes.Std;
using System;
public class Arm_view_Control : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button arm_view_mode;
    public Button front_view;
    public Button floor_view;
    public Button ceiling_view;
    public Button left_view;
    public Button Robot_home;
    public Button cancel;

    public Text viewStatusText;
    public GameObject panel;

    private string currentView = "None";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        panel.SetActive(false);
        arm_view_mode.onClick.AddListener(OnStartButtonclick);
        front_view.onClick.AddListener(() => OnviewButtonClick("frontview"));
        floor_view.onClick.AddListener(() => OnviewButtonClick("floorview"));
        ceiling_view.onClick.AddListener(() => OnviewButtonClick("ceilingview"));
        left_view.onClick.AddListener(() => OnviewButtonClick("leftview"));
        Robot_home.onClick.AddListener(()=> OnviewButtonClick("robothome"));
        cancel.onClick.AddListener(OnCancelButtonClick);
        UpdateViewStatus();
        
    }
    void OnviewButtonClick(string direction)
    {
        currentView = direction;
        string message = direction;
        StringMsg msg = new StringMsg(message);
        ros.Publish(topicName,msg);
        UpdateViewStatus();
    }
    void OnStartButtonclick()
    {
        panel.SetActive(true);
    }
    void OnCancelButtonClick()
    {
        panel.SetActive(false);
    }
    void UpdateViewStatus()
    {
        viewStatusText.text = "Robot Arm View: " + currentView;
    }

}
