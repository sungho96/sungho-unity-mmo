using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Map_Make : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public InputField Map_Number;
    public Button Map_make;
    
    public Button Map_make_pannel;
    public Button Map_Cancel;
    public Button Map_Scan;
    public Button cancel;
    public GameObject panel;

    void Start()
    {      
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROSConnection 인스턴스가 초기화되지 않았습니다!");
            return;
        }

        panel.SetActive(false);
        Map_make.onClick.AddListener(() => Make_map());
        cancel.onClick.AddListener(OnCancelButtonClick);
        Map_make_pannel.onClick.AddListener(OnStartButtonclick);
        Map_Cancel.onClick.AddListener(() => Make_cancel());
        Map_Scan.onClick.AddListener(() => Make_Sacn());
    }
    void Make_Sacn()
    {
        StringMsg message = new StringMsg("scan");
        ros.Publish(topicName,message);
    }
    void Make_cancel()
    {
        StringMsg message = new StringMsg("cancel");
        ros.Publish(topicName,message);
    }

    // Update is called once per frame
    void Make_map()
    {
        string Map_number_value = Map_Number.text;
        StringMsg message = new StringMsg($"fastlio_map; {Map_number_value}");
        ros.Publish(topicName, message);
    }

    void OnCancelButtonClick()
    {
        panel.SetActive(false);
    }

    void OnStartButtonclick()
    {
        panel.SetActive(true);
    }
}
