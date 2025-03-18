using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class Robot_Select : MonoBehaviour
{
    public GameObject panel;
    public Button GT3;
    public Button GT4;
    public Button cancel;
    public Button robot_select;
    public GameObject GT3_Robot;
    public GameObject GT4_Robot;
    void Start()
    {
        GT3_Robot.SetActive(true);
        GT4_Robot.SetActive(false);
        robot_select.onClick.AddListener(OnStartButtonclick);
        cancel.onClick.AddListener(OnCancelButtonClick);
        GT3.onClick.AddListener(OnSelectedGT3);
        GT4.onClick.AddListener(OnSelectedGT4);
    }
    void OnSelectedGT3()
    {
        GT3_Robot.SetActive(true);
        GT4_Robot.SetActive(false);
        panel.SetActive(false);
    }
    void OnSelectedGT4()
    {
        GT3_Robot.SetActive(false);
        GT4_Robot.SetActive(true);
        panel.SetActive(false);
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
