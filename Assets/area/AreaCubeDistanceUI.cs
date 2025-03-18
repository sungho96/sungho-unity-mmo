using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaCubeDistanceUI : MonoBehaviour
{
    public TextMeshProUGUI distanceText; // UI 텍스트
    public Transform robotTransform; // Robot 오브젝트
    private GameObject selectedBoundingCube = null; // 클릭된 BoundingCube 저장

    void Update()
    {
        if (robotTransform == null || distanceText == null)
            return;

        // 씬에 존재하는 모든 BoundingCube 찾기
        GameObject[] boundingCubes = GameObject.FindGameObjectsWithTag("BoundingCube");

        if (boundingCubes.Length == 0)
        {
            distanceText.text = "AreaCube None";
            return;
        }

        string distanceInfo = "AreaCube Distance\n";

        foreach (GameObject cube in boundingCubes)
        {
            float distance = Vector3.Distance(robotTransform.position, cube.transform.position);

            if (selectedBoundingCube == cube)
                distanceInfo += $"-> {cube.name}: {distance:F2}m <-\n"; // 클릭한 BoundingCube 강조
            else
                distanceInfo += $"{cube.name}: {distance:F2}m\n";
        }

        distanceText.text = distanceInfo;
    }

    // 클릭한 BoundingCube 설정 (강조)
    public void SetSelectedBoundingCube(GameObject cube)
    {
        if (selectedBoundingCube == cube)
        {
            selectedBoundingCube = null; // 다시 클릭하면 해제
        }
        else
        {
            selectedBoundingCube = cube;
        }
    }
}
