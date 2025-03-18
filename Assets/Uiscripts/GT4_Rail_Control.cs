using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using RosMessageTypes.Std;
using System;

public class GT4_Rail_Control : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "미정";
    public Button Rail_Control_Button;
    public Button Left_Button;
    public Button Right_Button;
    public Button Stop_Button;
    public Button Home_Button;
    public Button cancel;
    
    public Text CurrentlocationText;
    public GameObject panel;

    private string location = "Connecting";
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        panel.SetActive(false);
        UpdateViewStatus();
        Rail_Control_Button.onClick.AddListener(OnStartButtonclick);
        cancel.onClick.AddListener(OnCancelButtonClick);
        Left_Button.onClick.AddListener(() => OnviewButtonClick("left"));
        Right_Button.onClick.AddListener(() => OnviewButtonClick("right"));
        Stop_Button.onClick.AddListener(() => OnviewButtonClick("stop"));
        Home_Button.onClick.AddListener(() => OnviewButtonClick("home"));
    }
    void OnviewButtonClick(string direction)
    {
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
        CurrentlocationText.text = "Current Location: " + location;
    }


}
