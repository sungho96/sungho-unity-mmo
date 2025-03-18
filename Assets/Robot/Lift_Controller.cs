using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Lift_Controller : MonoBehaviour
{   ROSConnection ros;
    public string topicName = "/unity/cmd";
    public InputField Lift_Height;
    public Button Lift_Move;
    public Button Lift_Home;
    public Button Lift_Stop;
    public Button Lift_Active;
    public Button cancel;
    public Button Error_Cancel;
    public GameObject panel;
    public GameObject Error_Panel;

    void Start()
    {      
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROSConnection 인스턴스가 초기화되지 않았습니다!");
            return;
        }

        panel.SetActive(false);
        Error_Panel.SetActive(false);
        Lift_Move.onClick.AddListener(() => Lift_MoveOnButtonClick());
        cancel.onClick.AddListener(OnCancelButtonClick);
        Lift_Active.onClick.AddListener(OnStartButtonclick);
        Lift_Home.onClick.AddListener(() => Lift_HomeOnButtonClick());
        Lift_Stop.onClick.AddListener(() => Lift_StopOnButtonClick());
        Error_Cancel.onClick.AddListener(() => Lift_Error_CancelOnButtonClick());
    }

    void Lift_StopOnButtonClick()
    {
        StringMsg message = new StringMsg("lift_stop");
        ros.Publish(topicName,message);
    }
    void Lift_HomeOnButtonClick()
    {
        StringMsg message = new StringMsg("lift_home");
        ros.Publish(topicName,message);
    }

    // Update is called once per frame
    void Lift_MoveOnButtonClick()
    {
        if(float.TryParse(Lift_Height.text, out float liftHigh))
        {
            if(liftHigh >= 0 && liftHigh< 2.0f)
            {
                StringMsg message = new StringMsg($"lift_move; {liftHigh}");
                ros.Publish(topicName, message);
            }
            else
            {
                Error_Panel.SetActive(true); // 범위 벗어난 경우 오류 패널 표시
            } 
        }
        else
        {
            Error_Panel.SetActive(true); // 범위 벗어난 경우 오류 패널 표시
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
    void Lift_Error_CancelOnButtonClick()
    {
       Error_Panel.SetActive(false); 
    }
}
