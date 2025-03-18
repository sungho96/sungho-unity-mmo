using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 네임스페이스 추가
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class Tool_Select : MonoBehaviour
{
    public GameObject panel;
    public Button LongGun;
    public Button ShortGun;
    public Button cancel;
    public Button robot_select;
    public GameObject Long_Gun;
    public GameObject Short_Gun;
    void Start()
    {
        Long_Gun.SetActive(true);
        Short_Gun.SetActive(false);
        robot_select.onClick.AddListener(OnStartButtonclick);
        cancel.onClick.AddListener(OnCancelButtonClick);
        LongGun.onClick.AddListener(OnSelectedLongGun);
        ShortGun.onClick.AddListener(OnSelectedShortGun);
    }
    void OnSelectedLongGun()
    {
        Long_Gun.SetActive(true);
        Short_Gun.SetActive(false);
        panel.SetActive(false);
    }
    void OnSelectedShortGun()
    {
        Long_Gun.SetActive(false);
        Short_Gun.SetActive(true);
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
