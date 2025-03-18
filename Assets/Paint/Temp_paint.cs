using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Temp_paint : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public Button paint;
    public InputField Paint_Y_Number;
    public Button Paint_Y;

    public InputField Paint_OffSet_X_Number;
    public Button Paint_OffSet_X;

    public InputField Paint_Trigger_Start_Number;
    public Button Paint_Trigger_Start;
    
    public InputField Paint_Trigger_End_Number;
    public Button Paint_Trigger_End;


    public Button Paint_Active;
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
        Paint_Y_Number.text = "40.0";
        Paint_OffSet_X_Number.text = "20.0";
        Paint_Trigger_Start_Number.text = "100";
        Paint_Trigger_End_Number.text = "150";

        Paint_Active.onClick.AddListener(() => paint_pub());
        panel.SetActive(false);
        Paint_Y.onClick.AddListener(() => Paint_Y_Click());
        Paint_OffSet_X.onClick.AddListener(() => OffSet_X_Click());
        Paint_Trigger_Start.onClick.AddListener(() => Trigger_Start_Click());
        Paint_Trigger_End.onClick.AddListener(()=> Trigger_End_Click());
        cancel.onClick.AddListener(OnCancelButtonClick);
        paint.onClick.AddListener(OnStartButtonclick);
    }
    // Update is called once per frame
    void Paint_Y_Click()
    {
        if(float.TryParse(Paint_Y_Number.text, out float Number))
        {   if(Number >= -40.1 && Number<40.1)
            {
            StringMsg message = new StringMsg($"paint_y;{Number}");
            ros.Publish(topicName, message);
            }
        }
    }
    void OffSet_X_Click()
    {
        if(float.TryParse(Paint_OffSet_X_Number.text, out float Number))
        {   
            StringMsg message = new StringMsg($"offset_x;{Number}");
            ros.Publish(topicName, message);
        }
    }
    
    void Trigger_Start_Click()
    {
        if(float.TryParse(Paint_Trigger_Start_Number.text, out float Number))
        {   
            StringMsg message = new StringMsg($"trigger_start;{Number}");
            ros.Publish(topicName, message);
        }
    }
    
    void Trigger_End_Click()
    {
        if(float.TryParse(Paint_Trigger_Start_Number.text, out float Number))
        {   
            StringMsg message = new StringMsg($"trigger_End;{Number}");
            ros.Publish(topicName, message);
        }
    }
    void OnCancelButtonClick()
    {
        panel.SetActive(false);
    }

    void OnStartButtonclick()
    {
        panel.SetActive(true);
    }
    void paint_pub()
    {
        StringMsg message = new StringMsg("paint");
        ros.Publish(topicName,message);
    }
}
