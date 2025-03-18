using UnityEngine;

public class ScrollRotateZoom : MonoBehaviour
{
    public Camera Camera;
    public float scrollSpeed = 1.0f;   // 이동 속도
    public float zoomSpeed = 0.5f;     // 줌 속도
    public float rotationSpeed = 100f; // 회전 속도

    private Vector2 lastTouchPosition; // Vector2로 변경
    private float minZoom = 5f;  // 최소 줌 거리
    private float maxZoom = 50f; // 최대 줌 거리

    private float minYaw = -45f; // Y축 최소 회전 각도
    private float maxYaw = 45f;  // Y축 최대 회전 각도
    private float minPitch = -40f; // X축 최소 회전 각도
    private float maxPitch = 60f;  // X축 최대 회전 각도

    private bool isRotationMode = false; // 현재 회전 모드 여부
    void Start()
    {
        if (Camera == null)
        {
            Camera = Camera.main; // 메인 카메라 자동 할당
        }
    }
    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // 두 번 탭 시 모드 전환
                if (touch.tapCount == 2)
                {
                    isRotationMode = !isRotationMode; // 모드 전환
                }

                // 터치 시작 위치 저장
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // 이동 및 회전 계산
                Vector2 deltaPosition = touch.position - lastTouchPosition;

                if (isRotationMode)
                {
                    // 회전 모드 활성화
                    float rotationX = deltaPosition.y * rotationSpeed * Time.deltaTime;
                    float rotationY = -deltaPosition.x * rotationSpeed * Time.deltaTime;

                    // 현재 회전 값 가져오기
                    Vector3 currentRotation = Camera.transform.eulerAngles;

                    // Unity의 EulerAngles는 0~360도 범위로 표현되므로 -180~180도로 변환
                    float yaw = currentRotation.y > 180 ? currentRotation.y - 360 : currentRotation.y;
                    float pitch = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;

                    // 제한된 회전 각도 계산
                    yaw = Mathf.Clamp(yaw + rotationY, minYaw, maxYaw);
                    pitch = Mathf.Clamp(pitch + rotationX, minPitch, maxPitch);

                    // 회전 적용
                    Camera.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
                }
                else
                {
                    // 이동 모드
                    Vector3 movement = new Vector3(deltaPosition.x, 0, deltaPosition.y) * scrollSpeed * Time.deltaTime;
                    Camera.transform.Translate(movement, Space.Self);
                }

                // 현재 터치 위치 저장
                lastTouchPosition = touch.position;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // 두 손가락 간의 거리 계산
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float previousDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);

            float zoomDelta = (currentDistance - previousDistance) * zoomSpeed * Time.deltaTime;

            // 줌 적용 (카메라 필드 오브 뷰 조정)
            Camera.fieldOfView = Mathf.Clamp(Camera.fieldOfView - zoomDelta, minZoom, maxZoom);
        }
    }
}
