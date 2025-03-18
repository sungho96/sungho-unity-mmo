using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Point_Pub : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button point;

    void Start()
    {   ros = ROSConnection.GetOrCreateInstance();
        //point_pub();
        point.onClick.AddListener(() => point_pub());


    }
    void point_pub()
    {
        StringMsg message = new StringMsg("pc_refresh");
        ros.Publish(topicName,message);
    }
}
