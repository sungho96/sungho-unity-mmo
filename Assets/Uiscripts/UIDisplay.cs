using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisplay : MonoBehaviour
{
    public Text positionText;
    public Text rotationText;

    void OnEnable()
    {
        CombinedVisualizer.OnPositionUpdate += UpdatePositionText;
        CombinedVisualizer.OnRotationUpdate += UpdateRotationText;
    }

    void OnDisable()
    {
        CombinedVisualizer.OnPositionUpdate -= UpdatePositionText;
        CombinedVisualizer.OnRotationUpdate -= UpdateRotationText;
    }

    void UpdatePositionText(float x, float y, float z)
    {
        positionText.text = $"Position: X: {RoundToTwoDecimals(z)*-1}, Y: {RoundToTwoDecimals(x)}, Z: {RoundToTwoDecimals(y)*-1}";
        positionText.color = Color.white;  // 텍스트 색상을 흰색으로 강제 설정
    }

    void UpdateRotationText(float rotationY)
    {
        rotationText.text = $"Rotation Y: {RoundToTwoDecimals(rotationY)}";
        rotationText.color = Color.white;  // 텍스트 색상을 흰색으로 강제 설정
    }

    float RoundToTwoDecimals(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }
}
