using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Emergency : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button point;

    void Start()
    {   ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);
        point.onClick.AddListener(() => emergency_pub());


    }
    void emergency_pub()
    {
        StringMsg message = new StringMsg("mobile_emergency");
        ros.Publish(topicName,message);
    }
}
