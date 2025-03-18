using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class ScrollAndPinch : MonoBehaviour
{
    public Camera Camera;
    public float scrollSpeed = 1.0f;   // 이동 속도
    public float zoomSpeed = 1.0f;     // 줌 속도
    public float rotationSpeed = 100f; // 회전 속도

    private float minYaw = -45f; // Y축 최소 회전 각도
    private float maxYaw = 45f;  // Y축 최대 회전 각도
    private float minPitch = -180f; // X축 최소 회전 각도
    private float maxPitch = 180f;  // X축 최대 회전 각도

    protected Plane Plane;

    [SerializeField] 
    private Text debugText; // 인스펙터에서 연결


    private void Awake()
    {
        // 터치 평면 초기화
        Plane = new Plane(Vector3.up, Vector3.zero);
    }

    private void Update()
    {
        int currentTouchCount = Input.touchCount;
        
        // 현재 터치 수
        int touchCount = Input.touchCount;

        if (touchCount == 0)
        {
            debugText.text = "No Touches";
            return;
        }

        // 터치 정보 로그를 합쳐서 표시할 문자열
        string log = $"Touch Count: {touchCount}\n";
        
        for (int i = 0; i < touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // phase: Began, Moved, Stationary, Ended, Canceled
            // position: 화면 좌표 (0,0) ~ (Screen.width, Screen.height)
            // deltaPosition: 이전 프레임 대비 이동량
            log += $"[{i}] Phase: {t.phase}, Pos: {t.position}, Delta: {t.deltaPosition}\n";
        }

        debugText.text = log;

    }

    private void HandlePositionInput()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 currentMidPoint = (touch1.position + touch2.position) / 2;
            Vector2 previousMidPoint = ((touch1.position - touch1.deltaPosition) +
                                        (touch2.position - touch2.deltaPosition)) / 2;

            Vector3 delta = PlanePosition(currentMidPoint) - PlanePosition(previousMidPoint);
            Camera.transform.Translate(-delta * scrollSpeed, Space.World);

            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float previousDistance = Vector2.Distance(touch1.position - touch1.deltaPosition,
                                                       touch2.position - touch2.deltaPosition);

            float zoomFactor = (currentDistance - previousDistance) * zoomSpeed * 0.01f;
            Camera.transform.Translate(Vector3.forward * zoomFactor, Space.Self);
        }
    }

    private void HandleRotationInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                float rotationX = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;
                float rotationY = -touch.deltaPosition.x * rotationSpeed * Time.deltaTime;

                Vector3 currentRotation = Camera.transform.eulerAngles;

                float yaw = currentRotation.y > 180 ? currentRotation.y - 360 : currentRotation.y;
                float pitch = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;

                yaw = Mathf.Clamp(yaw + rotationY, minYaw, maxYaw);
                pitch = Mathf.Clamp(pitch + rotationX, minPitch, maxPitch);

                Camera.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }
        }
    }

    private Vector3 PlanePosition(Vector2 screenPos)
    {
        Ray ray = Camera.ScreenPointToRay(screenPos);
        if (Plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }
}
