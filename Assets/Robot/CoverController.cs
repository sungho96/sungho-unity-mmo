using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
public class CoverController : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button cover;
    public Button open;
    public Button close;
    public GameObject panel;
    public Button cancel;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        cover.onClick.AddListener(()=> OnStartButtonClick());
        cancel.onClick.AddListener(()=>OnCancelButtonClick());
        open.onClick.AddListener(()=> Cover_Open_Action());
        close.onClick.AddListener(()=> Cover_Close_Action());
        panel.SetActive(false);
    }

    void Cover_Open_Action()
    {
        StringMsg message = new StringMsg("cover_open");
        ros.Publish(topicName, message);
    }

    void Cover_Close_Action()
    {
        StringMsg message = new StringMsg("cover_close");
        ros.Publish(topicName, message);
    }
    void OnCancelButtonClick()
    {
        panel.SetActive(false);
    }

    void OnStartButtonClick()
    {
        panel.SetActive(true);
    }

}
