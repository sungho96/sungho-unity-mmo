using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class test_Pub : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button point;

    void Start()
    {   ros = ROSConnection.GetOrCreateInstance();
        point.onClick.AddListener(() => point_pub());


    }
    void point_pub()
    {
        StringMsg message = new StringMsg("saved_motion_setting");
        ros.Publish(topicName,message);
    }
}
