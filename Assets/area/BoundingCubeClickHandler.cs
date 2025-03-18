using UnityEngine;

public class BoundingCubeClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        // 클릭된 오브젝트가 BoundingCube인지 확인
        if (!gameObject.CompareTag("BoundingCube"))
            return;

        // UI 매니저를 찾아서 클릭 정보 전달
        AreaCubeDistanceUI uiManager = FindObjectOfType<AreaCubeDistanceUI>();
        if (uiManager != null)
        {
            uiManager.SetSelectedBoundingCube(gameObject);
        }
    }
}
