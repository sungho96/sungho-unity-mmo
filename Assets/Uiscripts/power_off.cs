using UnityEngine;
using UnityEngine.UI;

public class PowerOffManager : MonoBehaviour
{
    public GameObject powerOffPanel;
    public Button yesButton;
    public Button noButton;
    public Button powerOffButton;

    void Start()
    {
        // 패널을 비활성화합니다.
        powerOffPanel.SetActive(false);

        // 버튼 클릭 이벤트를 등록합니다.
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
        powerOffButton.onClick.AddListener(ShowPowerOffPanel);
    }

    // Power Off 패널을 표시하는 함수
    public void ShowPowerOffPanel()
    {
        powerOffPanel.SetActive(true);
    }

    // 예 버튼 클릭 시 호출되는 함수
    void OnYesButtonClicked()
    {
        Application.Quit();
    }

    // 아니오 버튼 클릭 시 호출되는 함수
    void OnNoButtonClicked()
    {
        powerOffPanel.SetActive(false);
    }
}
