using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ConnectedLineInfo
{
    public LineRenderer lineRenderer;  // 연결된 선
    public CubeDraggable otherCube;    // 연결된 다른 큐브
    public TextMesh textMesh;          // 연결된 텍스트
}

public class CubeDraggable : MonoBehaviour
{
    public List<ConnectedLineInfo> connectedLines = new List<ConnectedLineInfo>();
    private Vector3 movementAxis = Vector3.zero;
    private Vector3 initialMousePosition;
    private Vector3 initialCubePosition;
    private Plane dragPlane;

    // currentView 변수를 추가합니다.
    private string currentView;

    private GameObject parentGroup; // 부모 그룹 참조 추가


    // currentView를 설정하는 메서드를 추가합니다.
    public void SetCurrentView(string view)
    {
        currentView = view;
    }
    void Start()
    {
        parentGroup = transform.parent.gameObject; // CubeGroup 참조
    }
    void OnMouseDown()
    {
        if (IsPointerOverUI()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        dragPlane = new Plane(Vector3.up, parentGroup.transform.position.y);
        if (dragPlane.Raycast(ray, out float enter))
        {
            initialMousePosition = ray.GetPoint(enter);
            initialCubePosition = parentGroup.transform.position;
        }
    }
    void OnMouseDrag()
    {
        if (IsPointerOverUI()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            //클릭한 위치(initialMousePosition)와의 차이를 계산하여 이동 (순간이동 방지)
            float scaleFactor = 0.5f;
            Vector3 offset = (hitPoint - initialMousePosition) * scaleFactor;
            Vector3 newPosition = initialCubePosition + new Vector3(offset.x, 0, offset.z);

            // Y 값은 그대로 유지하면서 자연스럽게 이동
            parentGroup.transform.position = newPosition;
        }
    }
    void Update()
    {
        UpdateCurrentview();
        UpdateConnectedObjects();
    }
    void UpdateCurrentview()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        
        if (Vector3.Dot(cameraForward, Vector3.down)> 0.9f)
        {
            currentView ="Top Down";
        }
        else if ( Vector3.Dot(cameraForward, Vector3.forward) >0.9f)
        {
            currentView ="Front";
        }
        else if (Vector3.Dot(cameraForward, Vector3.right)> 0.9f)
        {
            currentView = "Right";
        }
        else if (Vector3.Dot(cameraForward, Vector3.left)> 0.9f)
        {
            currentView = "Left";
        }
        else
        {
            currentView= "Front";
        }
    }

    // 연결된 선과 텍스트를 업데이트하는 함수
    void UpdateConnectedObjects()
    {
        foreach (ConnectedLineInfo lineInfo in connectedLines)
        {
            // 현재 큐브 위치에 y 좌표 조정
            Vector3 adjustedPosition = transform.position;
            adjustedPosition.y += 0.01f; // y 좌표를 약간 올려줍니다.

            // 연결된 다른 큐브 위치에 y 좌표 조정
            Vector3 adjustedOtherPosition = lineInfo.otherCube.transform.position;
            adjustedOtherPosition.y += 0.01f; // y 좌표를 약간 올려줍니다.

            // 선의 양 끝점을 조정된 위치로 설정
            lineInfo.lineRenderer.SetPosition(0, adjustedPosition);
            lineInfo.lineRenderer.SetPosition(1, adjustedOtherPosition);

            // 텍스트 위치 업데이트 (조정된 중간 지점 사용)
            Vector3 midPoint = (adjustedPosition + adjustedOtherPosition) / 2;
            lineInfo.textMesh.transform.position = midPoint;

            // 텍스트 내용 업데이트
            float distance = Vector3.Distance(transform.position, lineInfo.otherCube.transform.position);
            lineInfo.textMesh.text = distance.ToString("F2") + "m";

            // 텍스트 회전 업데이트
            UpdateTextRotation(lineInfo.textMesh);
        }
    }

    // 텍스트의 회전을 업데이트하는 함수
    void UpdateTextRotation(TextMesh textMesh)
    {
        if (currentView == "Top Down")
        {
            textMesh.transform.rotation = Quaternion.Euler(90, 90, 90);
        }
        else if (currentView == "Front")
        {
            textMesh.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currentView == "Right")
        {
            textMesh.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (currentView == "Left")
        {
            textMesh.transform.rotation = Quaternion.Euler(0, -90, 0);
        }
        else
        {
            // 기본적으로 카메라를 향하도록 설정
            textMesh.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    // 마우스가 UI 요소 위에 있는지 확인하는 함수
    bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
