using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Paint : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button paint;

    void Start()
    {   ros = ROSConnection.GetOrCreateInstance();
        paint.onClick.AddListener(() => paint_pub());
    }
    void paint_pub()
    {
        StringMsg message = new StringMsg("paint");
        ros.Publish(topicName,message);
    }
}
