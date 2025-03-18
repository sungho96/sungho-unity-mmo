using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

public class RobotPoseController : MonoBehaviour
{
    public ROSConnection ros;
    public InputField fkX, fkY, fkZ, fkRX, fkRY, fkRZ;
    public InputField ikX, ikY, ikZ, ikRX, ikRY, ikRZ;
    public Button fkApplyButton, ikApplyButton, currentPoseButton, RobotPoseButton, PosePanelCancelButton;
    public GameObject panel;

    private string pubTopicName = "/robot_pose";
    private string subTopicName = "/current_pose";
    private bool isSubscribed = false; // 버튼을 눌러야 true가 됨

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        panel.SetActive(false);
        fkApplyButton.onClick.AddListener(() => ApplyPose("fk"));
        ikApplyButton.onClick.AddListener(() => ApplyPose("ik"));
        currentPoseButton.onClick.AddListener(RequestCurrentPose);
        RobotPoseButton.onClick.AddListener(() => OnStartButtonclick());
        PosePanelCancelButton.onClick.AddListener(() => OnCancelButtonclick());

        // 계속 구독 유지하되 isSubscribed이 true일 때만 데이터 처리
        ros.Subscribe<StringMsg>(subTopicName, OnPoseReceived);
    }

    void RequestCurrentPose()
    {
        if (isSubscribed)
        {
            Debug.Log("이미 데이터 수신 중...");
            return;
        }

        Debug.Log("현재 포즈 요청 시작!");
        isSubscribed = true;  // 이제 한 번 데이터를 받을 준비 완료

        // 0.1초 후 다시 false로 변경하여 무효 데이터 무시
        StartCoroutine(ResetSubscriptionFlag());
    }

    void OnPoseReceived(StringMsg msg)
    {
        if (!isSubscribed) 
        {
            return;
        }

        Debug.Log($"수신된 데이터 처리 중: {msg.data}");

        string[] poseSets = msg.data.Split(';');

        if (poseSets.Length >= 2)
        {
            string[] ikValues = poseSets[0].Trim('(', ')').Split(',');
            string[] fkValues = poseSets[1].Trim('(', ')').Split(',');

            if (ikValues.Length >= 6 && fkValues.Length >= 6)
            {
                ikX.text = Mathf.RoundToInt(float.Parse(ikValues[0])).ToString();
                ikY.text = Mathf.RoundToInt(float.Parse(ikValues[1])).ToString();
                ikZ.text = Mathf.RoundToInt(float.Parse(ikValues[2]) + 1200).ToString();
                ikRX.text = Mathf.RoundToInt(float.Parse(ikValues[3])).ToString();
                ikRY.text = Mathf.RoundToInt(float.Parse(ikValues[4])).ToString();
                ikRZ.text = Mathf.RoundToInt(float.Parse(ikValues[5])).ToString();

                fkX.text = Mathf.RoundToInt(float.Parse(fkValues[0])).ToString();
                fkY.text = Mathf.RoundToInt(float.Parse(fkValues[1])).ToString();
                fkZ.text = Mathf.RoundToInt(float.Parse(fkValues[2])).ToString();
                fkRX.text = Mathf.RoundToInt(float.Parse(fkValues[3])).ToString();
                fkRY.text = Mathf.RoundToInt(float.Parse(fkValues[4])).ToString();
                fkRZ.text = Mathf.RoundToInt(float.Parse(fkValues[5])).ToString();
            }
        }
        else
        {
            Debug.LogWarning("잘못된 데이터 형식 수신!");
        }
    }

    IEnumerator ResetSubscriptionFlag()
    {
        yield return new WaitForSeconds(0.5f); 
        isSubscribed = false;
    }

    void ApplyPose(string mode)
    {
        float x, y, z, rx, ry, rz;
        string message;
        if (mode == "fk")
        {
            x = float.Parse(fkX.text);
            y = float.Parse(fkY.text);
            z = float.Parse(fkZ.text);
            rx = float.Parse(fkRX.text);
            ry = float.Parse(fkRY.text);
            rz = float.Parse(fkRZ.text);
            message = $"{mode};J;[{x},{y},{z},{rx},{ry},{rz},20,20]";
        }
        else
        {
            x = float.Parse(ikX.text);
            y = float.Parse(ikY.text);
            z = float.Parse(ikZ.text) - 1200;
            rx = float.Parse(ikRX.text);
            ry = float.Parse(ikRY.text);
            rz = float.Parse(ikRZ.text);
            message = $"{mode};L;[{x},{y},{z},{rx},{ry},{rz},400,400]";
        }

        PublishMessage(message);
    }

    void PublishMessage(string message)
    {
        Debug.Log($"발행: {pubTopicName} -> {message}");
        ros.Publish(pubTopicName, new StringMsg(message));
    }

    void OnStartButtonclick()
    {
        panel.SetActive(true);
    }

    void OnCancelButtonclick()
    {
        panel.SetActive(false);
    }
}
