using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using TMPro; // TextMeshPro 네임스페이스 추가

public class MotionSetting : MonoBehaviour
{
    ROSConnection ros;
    //gt3/motion_setting u자 changed_motion_setting;[0.2,5,300,500,3]
    public String SubTopicName = "gt3/motion_setting";
    public String topicName = "unity/cmd";
    
    public InputField Spray_distance;
    public InputField Height;
    public InputField Motion_Speed;
    public InputField Acceleration;
    public InputField way_point;
    public Toggle SideToggle;
    public Toggle RealToggle;
    public Toggle PaintGun;
    public Toggle wall;
    public Button Motion_Setting;
    public Button Cancel;
    public Button Apply;
    public Button Save;
    public GameObject Panel;
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        Panel.SetActive(false);
        SideToggle.onValueChanged.AddListener(onsideToggleChange);
        RealToggle.onValueChanged.AddListener(onRealSimulChange);
        PaintGun.onValueChanged.AddListener(onPaintChange);
        wall.onValueChanged.AddListener(onWallChange);
        Cancel.onClick.AddListener(Cancel_Action);
        Motion_Setting.onClick.AddListener(Start_Action);
        Apply.onClick.AddListener(() => Apply_action());
        Save.onClick.AddListener(() => Save_action());
        Init_Request_Motion_Data();    
    }
    void onsideToggleChange(bool isOn)
    {
        string message = isOn ? "sdie_c": "side_nc";
        StringMsg rosMessage = new StringMsg(message);
        ros.Publish(topicName, rosMessage);
    }
    void onRealSimulChange(bool isOn)
    {
        string message = isOn ? "real": "simul";
        StringMsg rosMessage = new StringMsg(message);
        ros.Publish(topicName, rosMessage);
    }
    void onPaintChange(bool isOn)
    {
        string message = isOn ? "pull": "unpull";
        StringMsg rosMessage = new StringMsg(message);
        ros.Publish(topicName, rosMessage);
    }
    void onWallChange(bool isOn)
    {
        string message = isOn ? "Wall": "not_wall";
        StringMsg rosMessage = new StringMsg(message);
        ros.Publish(topicName, rosMessage);
    }

    void Apply_action()
    {
        string Spray_distance_Value = Spray_distance.text;
        string Height_Value= Height.text;
        string Motion_Speed_Value= Motion_Speed.text;
        string Acceleration_Value= Acceleration.text;
        string way_point_value= way_point.text;

        StringMsg message = new StringMsg($"changed_motion_setting;[{Spray_distance_Value},{Height_Value},{Motion_Speed_Value},{Acceleration_Value},{way_point_value}]");
        ros.Publish(topicName, message);

        Debug.Log($"Sent to ROS: {message.data}");
    }
        void Save_action()
    {
        string Spray_distance_Value = Spray_distance.text;
        string Height_Value= Height.text;
        string Motion_Speed_Value= Motion_Speed.text;
        string Acceleration_Value= Acceleration.text;
        string way_point_value= way_point.text;

        StringMsg message = new StringMsg($"save_motion_setting;[{Spray_distance_Value},{Height_Value},{Motion_Speed_Value},{Acceleration_Value},{way_point_value}]");
        ros.Publish(topicName, message);

        Debug.Log($"Sent to ROS: {message.data}");
    }
    
    void Cancel_Action()
    {
        Panel.SetActive(false);
    }
    void Start_Action()
    {
        Panel.SetActive(true);
    }
    void Init_Request_Motion_Data()
    {
        StringMsg message = new StringMsg("saved_motion_setting");
        ros.Publish(topicName,message);
        ros.Subscribe<StringMsg>(SubTopicName,Init_Motion_Data);
    }
    void Init_Motion_Data(StringMsg message)
    {
        string[] values = message.data.Trim('[', ']').Split(',');
        if (values.Length == 5)
        {
            Spray_distance.text = values[0];
            Height.text = values[1];
            Motion_Speed.text = values[2];
            Acceleration.text = values[3];
            way_point.text = values[4];
            Debug.Log($"{string.Join(", ", values)}");
        }
        else
        {
            Debug.LogError($"Received data does not match the expected format: {message.data}");
        }
    }

}
