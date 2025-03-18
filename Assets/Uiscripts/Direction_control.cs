using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class Direction_control : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/unity/cmd";
 
    public InputField Distance;
    public InputField RPM;
    public InputField AngleDegree;
    public InputField BackWheelangle;

    public Button go_forward; // 시작 버튼
    public Button go_backward; // 후진 버튼
    public Button right_forward; 
    public Button left_forward;
    public Button right_backward;
    public Button left_backward;    
    public Button Pause;
    public Button Stop;
    
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        
        // 퍼블리셔 등록
        ros.RegisterPublisher<StringMsg>(topicName);
        
        RPM.text = "10";
        BackWheelangle.text = "55";

        go_forward.onClick.AddListener(() => Type1("go_forward"));
        go_backward.onClick.AddListener(() => Type1("go_backward"));
        right_forward.onClick.AddListener(() => Type1("right_forward"));
        right_backward.onClick.AddListener(() => Type1("right_backward"));
        left_forward.onClick.AddListener(() => Type1("left_forward"));
        left_backward.onClick.AddListener(() => Type1("left_backward"));
        Pause.onClick.AddListener(OnPauseButtonClick);
        Stop.onClick.AddListener(OnStopButtonClick);
    }

    void Type1(string direction)
    {
        string distanceValue = Distance.text;
        string rpmValue = RPM.text;
        string angleDegreeValue = AngleDegree.text;
        string backWheelAngleValue = BackWheelangle.text;
        
        string message_1 ="start";
        string message = $"{direction};{distanceValue};{rpmValue};{angleDegreeValue};{backWheelAngleValue}";

        StringMsg msg = new StringMsg(message);
        StringMsg msg_1 = new StringMsg(message_1);
        ros.Publish(topicName,msg_1);
        ros.Publish(topicName, msg);

        Debug.Log($"Direction: {direction},Distance: {distanceValue}, RPM: {rpmValue}, AngleDegree: {angleDegreeValue}, BackWheelAngle: {backWheelAngleValue}");
    }

    void OnPauseButtonClick()
    {
        StringMsg msg;
        TextMeshProUGUI buttonText = Pause.GetComponentInChildren<TextMeshProUGUI>();
        if (isPaused)
        {
            msg = new StringMsg("resume");
            isPaused = false;
            buttonText.text = "Pause";
        }
        else
        {
            msg = new StringMsg("pause");
            isPaused = true;
            buttonText.text = "Resume";
        }

        ros.Publish(topicName, msg);
    }

    void OnStopButtonClick()
    {
        StringMsg msg = new StringMsg("stop");
        TextMeshProUGUI buttonText = Pause.GetComponentInChildren<TextMeshProUGUI>();
        ros.Publish(topicName, msg);
        isPaused = false;
        buttonText.text = "Pause";
    }

}
