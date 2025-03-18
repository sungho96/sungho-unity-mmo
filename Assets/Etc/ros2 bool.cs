using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class ros2_bool : MonoBehaviour
{   
    ROSConnection ros;
    public string topicName = "/mobile/move_flag";
    public Button Move_Button;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<BoolMsg>(topicName);

        if (Move_Button != null)
        {
            Move_Button.onClick.AddListener(() => Mobile_Move_Action());
        }
    }
    
    void Mobile_Move_Action()
    {
        BoolMsg message = new BoolMsg(true);
        ros.Publish(topicName, message);
    }
}
