using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class PoseController : MonoBehaviour
{
    // TMP_Text와 Button 선언
    public TMP_Text xText, yText, zText, rxText, ryText, rzText;
    public Button xPlusButton, xMinusButton;
    public Button yPlusButton, yMinusButton;
    public Button zPlusButton, zMinusButton;
    public Button rxPlusButton, rxMinusButton;
    public Button ryPlusButton, ryMinusButton;
    public Button rzPlusButton, rzMinusButton;

    private ROSConnection ros;
    public string subTopicName = "robot/current_pose"; // ROS에서 수신할 토픽 이름
    public string pubTopicName = "robot/updated_pose"; // ROS로 발행할 토픽 이름

    private float xValue = 0, yValue = 0, zValue = 0;
    private float rxValue = 0, ryValue = 0, rzValue = 0;
    private const float stepValue = 5.0f; // 버튼 클릭 시 증가/감소 값

    void Start()
    {
        // ROS 연결 및 토픽 구독
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>(subTopicName, UpdatePose);

        // 버튼 이벤트 추가
        xPlusButton.onClick.AddListener(() => AdjustValue(ref xValue, stepValue, xText));
        xMinusButton.onClick.AddListener(() => AdjustValue(ref xValue, -stepValue, xText));

        yPlusButton.onClick.AddListener(() => AdjustValue(ref yValue, stepValue, yText));
        yMinusButton.onClick.AddListener(() => AdjustValue(ref yValue, -stepValue, yText));

        zPlusButton.onClick.AddListener(() => AdjustValue(ref zValue, stepValue, zText));
        zMinusButton.onClick.AddListener(() => AdjustValue(ref zValue, -stepValue, zText));

        rxPlusButton.onClick.AddListener(() => AdjustValue(ref rxValue, stepValue, rxText));
        rxMinusButton.onClick.AddListener(() => AdjustValue(ref rxValue, -stepValue, rxText));

        ryPlusButton.onClick.AddListener(() => AdjustValue(ref ryValue, stepValue, ryText));
        ryMinusButton.onClick.AddListener(() => AdjustValue(ref ryValue, -stepValue, ryText));

        rzPlusButton.onClick.AddListener(() => AdjustValue(ref rzValue, stepValue, rzText));
        rzMinusButton.onClick.AddListener(() => AdjustValue(ref rzValue, -stepValue, rzText));
    }

    void UpdatePose(StringMsg message)
    {
        // 수신된 메시지를 쉼표로 나눔
        string[] poseValues = message.data.Split(',');

        // 데이터가 유효한지 확인
        if (poseValues.Length == 6)
        {
            // 값 파싱
            xValue = float.Parse(poseValues[0]);
            yValue = float.Parse(poseValues[1]);
            zValue = float.Parse(poseValues[2]);
            rxValue = float.Parse(poseValues[3]);
            ryValue = float.Parse(poseValues[4]);
            rzValue = float.Parse(poseValues[5]);

            // UI 업데이트
            UpdateUI();
        }
        else
        {
            Debug.LogError("Invalid pose data received: " + message.data);
        }
    }

    void AdjustValue(ref float value, float adjustment, TMP_Text textField)
    {
        value += adjustment; // 값 조정
        textField.text = value.ToString("F1"); // 소수점 1자리로 표시

        PublishUpdatedPose(); // 값이 변경되었으므로 ROS로 전체 값 발행
    }

    void PublishUpdatedPose()
    {
        // 현재 값을 쉼표로 구분된 문자열로 변환
        string updatedPose = $"{xValue:F1},{yValue:F1},{zValue:F1},{rxValue:F1},{ryValue:F1},{rzValue:F1}";

        // ROS 메시지 생성 및 발행
        StringMsg rosMessage = new StringMsg(updatedPose);
        ros.Publish(pubTopicName, rosMessage);

        Debug.Log($"Published updated pose: {updatedPose}");
    }

    void UpdateUI()
    {
        xText.text = xValue.ToString("F1");
        yText.text = yValue.ToString("F1");
        zText.text = zValue.ToString("F1");
        rxText.text = rxValue.ToString("F1");
        ryText.text = ryValue.ToString("F1");
        rzText.text = rzValue.ToString("F1");
    }
}
