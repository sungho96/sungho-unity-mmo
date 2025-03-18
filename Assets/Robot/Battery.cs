using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class BatteryDisplay : MonoBehaviour
{
    [SerializeField]
    private Text batteryText; // 배터리 상태를 표시할 텍스트 (항상 흰색)
    [SerializeField]
    private Text TempText; // 배터리 상태를 표시할 텍스트 (항상 흰색)
    [SerializeField]
    private Image batteryIcon; 
    [SerializeField]
    private GameObject FullBattery_panel; 
    [SerializeField]
    private GameObject LowBattery_panel; 
    [SerializeField]
    private Button fullPanel_Cancel; 
    [SerializeField]
    private Button LowPanel_Cancel; 

    private bool Fullchecked = true;
    private bool Lowchecked = true;

    private ROSConnection ros;

    private Color orange = new Color(1.0f, 0.5f, 0.0f); // RGB: (255, 128, 0)

    void Start()
    {
        // ROS Connection 초기화
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Int32Msg>("/mobile/battery", UpdateBatteryDisplay);
        ros.Subscribe<Int32Msg>("/mobile/temperature",UpdatTemperatureDisplay);
        fullPanel_Cancel.onClick.AddListener(Cancel_fullBatteryPanel);
        LowPanel_Cancel.onClick.AddListener(Cancel_LowBatteryPanel);
    }

    void UpdateBatteryDisplay(Int32Msg batteryMsg)
    {
        int batteryLevel = batteryMsg.data;

        // 텍스트 업데이트 (항상 흰색)
        batteryText.text = $"Battery: {batteryLevel}%";
        batteryText.color = Color.white;

        batteryIcon.fillAmount = Mathf.Clamp01(batteryLevel / 100f); 

        // 배터리 수준에 따라 아이콘 색상 변경
        if (batteryLevel==100)
        {
            batteryIcon.color = Color.green; // 녹색
            SetactivefullBatery();

        }
        else if (99>=batteryLevel || batteryLevel > 70)
        {
            batteryIcon.color = Color.green; // 녹색
            Fullchecked = true;
        }
        else if (batteryLevel > 33)
        {
            batteryIcon.color = orange; // 노란색
        }
        else if (batteryLevel == 15)
        {
            SetactiveLowBatery();
        }
        else if (batteryLevel >= 16)
        {     
            Lowchecked = true;
            batteryIcon.color = Color.red; 
        }
        else
        {
            batteryIcon.color = Color.red; // 빨간색
        }
    }
    void UpdatTemperatureDisplay(Int32Msg TempMsg)
    {
        int batteryTemp = TempMsg.data;
        TempText.text = $"Battery_Temp: {batteryTemp}°C";
        batteryText.color = Color.white;
    }
    void SetactivefullBatery()
    {
        if (Fullchecked)
        {
        FullBattery_panel.SetActive(true);
        }
    }
    void SetactiveLowBatery()
    {   if (Lowchecked)
        {
        LowBattery_panel.SetActive(true);
        }
    }
    void Cancel_fullBatteryPanel()
    {
        FullBattery_panel.SetActive(false);
        Fullchecked = false;
    }
    void Cancel_LowBatteryPanel()
    {
        LowBattery_panel.SetActive(false);
        Lowchecked =false;
    }
}
