using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class Mobile_Move : MonoBehaviour
{   
    ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button Move_Button;
    public Toggle Move_Auto_Maunal;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        if (Move_Button != null)
        {
            Move_Button.onClick.AddListener(() => Mobile_Move_Action());
        }
        Move_Auto_Maunal.onValueChanged.AddListener(onStatusToggleChange);
        onStatusToggleChange(Move_Auto_Maunal.isOn);
    }
    
    void Mobile_Move_Action()
    {
        StringMsg message = new StringMsg("mobile_move");
        ros.Publish(topicName, message);
    }
    void onStatusToggleChange(bool isOn)
    {
        string message = isOn ? "move_auto": "move_manual";
        StringMsg rosMessage = new StringMsg(message);
        ros.Publish(topicName, rosMessage);
    }
}
