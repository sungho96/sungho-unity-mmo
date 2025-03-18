using UnityEngine;
using UnityEngine.UI;

public class CameraTransformChanger : MonoBehaviour
{
    public Button unity_camera_view_mode;
    public Text viewStatusText;
    public GameObject panel;
    public Button Top_Down_view;
    public Button Back_view;
    public Button Right_view;
    public Button left_view;
    public Button cancel;
    public Button Home_button;

    public Transform robotTransform;
    public float distanceBehindRobot = 5f;
    public float heightAboveRobot = 2f;
    private string currentView = "Front";
    private Camera mainCamera;
    private Quaternion targetRotation;
    private float rotationSpeed = 1f; // 카메라 이동 시 회전 속도
    
    // Camera_TF 스크립트 참조
    public CombinedVisualizer cameraTFScript;

    // 기본 오프셋 값
    private Vector3 defaultPositionOffset = new Vector3(0.26f, 1.58f, -4.71f);
    private Vector3 defaultRotationOffset = new Vector3(0f, 0f, 0f);

    // 마지막으로 설정된 오프셋 값
    private Vector3 currentPositionOffset;
    private Vector3 currentRotationOffset;

    // 줌 관련 변수
    public float zoomSpeed = 500f;
    public float minZoom = 15f;
    public float maxZoom = 75f;
    private float targetZoom;
    private float zoomLerpSpeed = 20f; 
    private bool isCameraFree = false;
    private Vector3 storedPos;
    private bool isPositionStored = false;
    private string previousView = ""; 

    void Start()
    {
        mainCamera = Camera.main;
        targetRotation = mainCamera.transform.rotation;
        panel.SetActive(false);

        unity_camera_view_mode.onClick.AddListener(OnStartButtonclick);
        cancel.onClick.AddListener(OnCancelButtonClick);
        Home_button.onClick.AddListener(ResetCameraToHome);

        Top_Down_view.onClick.AddListener(SetTopDownView);
        Back_view.onClick.AddListener(SetBackView);
        left_view.onClick.AddListener(SetLeftView);
        Right_view.onClick.AddListener(SetRightView);

        currentPositionOffset = defaultPositionOffset;
        currentRotationOffset = defaultRotationOffset;
        targetZoom = mainCamera.fieldOfView;
        UpdateViewStatus();
    }

    void Update()
    {
        if (!isCameraFree)
        {
            // 목표 위치와 회전을 매 프레임마다 업데이트
            float targetPosition_x = cameraTFScript.storedEulerAngles.x;
            float targetPosition_y = cameraTFScript.storedEulerAngles.y;
            float targetPosition_z = cameraTFScript.storedEulerAngles.z;

            if (currentView == "Left")
            {
                targetPosition_y -= 90.0f;
            }
            else if (currentView == "Right")
            {
                targetPosition_y += 90.0f;
            }
            else if (currentView == "Top Down")
            {
                targetPosition_x += 90.0f;
                targetPosition_y += 180.0f;
                // 필요하다면 Z축도 조정
            }

            Vector3 adjustedRotation = new Vector3(
                targetPosition_x + currentRotationOffset.x, 
                targetPosition_y + currentRotationOffset.y, 
                targetPosition_z + currentRotationOffset.z
            );

            float storedRadian = cameraTFScript.storedEulerAngles.y * Mathf.Deg2Rad * -1;

            float x_2 = currentPositionOffset.x * Mathf.Cos(storedRadian) - currentPositionOffset.z * Mathf.Sin(storedRadian);
            float z_2 = currentPositionOffset.x * Mathf.Sin(storedRadian) + currentPositionOffset.z * Mathf.Cos(storedRadian);

            Vector3 adjustedPosition = new Vector3(x_2, currentPositionOffset.y, z_2) + cameraTFScript.storedPosition;

            targetRotation = Quaternion.Euler(adjustedRotation);

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, adjustedPosition, Time.deltaTime * rotationSpeed);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            bool positionReached = Vector3.Distance(mainCamera.transform.position, adjustedPosition) < 0.1f;
            bool rotationReached = Quaternion.Angle(mainCamera.transform.rotation, targetRotation) < 0.1f;

            if (positionReached && rotationReached)
            {
                mainCamera.transform.rotation = targetRotation;
                mainCamera.transform.position = adjustedPosition;
                isCameraFree = true;
            }
        }
        else
        {
            // 자유 이동 관련 처리(있다면)
            HandleFreeCameraMovement();
        }
        
        // 뷰 변경 감지
        if (currentView != previousView)
        {   
            isPositionStored = false;  
            targetZoom = 74; 
            previousView = currentView;
        }

        float scrolldata = Input.GetAxis("Mouse ScrollWheel");

        if (scrolldata != 0.0f)
        {
            // targetZoom < 75일 때는 fieldOfView로 부드럽게 줌
            if (targetZoom < 75f)
            {
                // 혹시 카메라가 원복 중이라면 위치 복귀가 거의 끝났을 때 FOV 변경 시작
                if (isPositionStored && Vector3.Distance(mainCamera.transform.position, storedPos) < 0.01f)
                {
                    isPositionStored = false; 
                    targetZoom = 73; 
                }
                else if (!isPositionStored)
                {
                    // 이동 완료 후 FOV 조정
                    targetZoom -= scrolldata * (zoomSpeed * 0.5f) * Time.deltaTime;
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, 75f);
                }
                // 만약 isPositionStored == true 면, 계속 복귀 중이므로 위치 이동 코드만 계속 수행
            }
        }

        // 1) targetZoom >= 75 && scrolldata < 0.0f → 줌 아웃(멀어지기) 로직
        if (targetZoom >= 75f && scrolldata < 0.0f)
        {
            // 한 번만 저장
            if (!isPositionStored)
            {
                storedPos = mainCamera.transform.position;
                isPositionStored = true;
            }
            // 카메라를 뷰 방향으로 멀어지도록 이동
            Vector3 moveBack = mainCamera.transform.position;

            // 뷰 별 "멀어지기" 방향(+= 또는 -=) 설정 예시 
            if (currentView == "Front" || currentView == "Back")
            {
                
                moveBack.z -= (Mathf.Abs(scrolldata) * (zoomSpeed * 0.1f)) * Time.deltaTime;
            }
            else if (currentView == "Right")
            {
                
                moveBack.x -= (Mathf.Abs(scrolldata) * (zoomSpeed * 0.1f)) * Time.deltaTime;
            }
            else if (currentView == "Left")
            {
                
                moveBack.x += (Mathf.Abs(scrolldata) * (zoomSpeed * 0.1f)) * Time.deltaTime;
            }
            else if (currentView == "Top Down")
            {
                
                moveBack.y += (Mathf.Abs(scrolldata) * (zoomSpeed * 0.1f)) * Time.deltaTime;
            }


            mainCamera.transform.position = moveBack;
        }

        // 2) targetZoom >= 75 && scrolldata > 0.0f && isPositionStored → 줌 인(다시 원래 위치로)
        if (targetZoom >= 75f && scrolldata > 0.0f && isPositionStored)
        {
            Vector3 moveForward = mainCamera.transform.position;

            
            if (currentView == "Front" || currentView == "Back")
            {
                // Z축 원복
                moveForward.z = Mathf.MoveTowards(moveForward.z, storedPos.z, (zoomSpeed * 0.01f) * Time.deltaTime);
            }
            else if (currentView == "Right")
            {
                // Right 뷰 → -X쪽 원복
                moveForward.x = Mathf.MoveTowards(moveForward.x, storedPos.x, (zoomSpeed * 0.01f) * Time.deltaTime);
            }
            else if (currentView == "Left")
            {
                // Left 뷰 → +X쪽 원복
                // MoveTowards의 3번째 인자( maxDelta )는 항상 양수로
                moveForward.x = Mathf.MoveTowards(moveForward.x, storedPos.x, (zoomSpeed * 0.01f) * Time.deltaTime);
            }
            else if (currentView == "Top Down")
            {
                // TopDown 뷰 → Y축 원복
                moveForward.y = Mathf.MoveTowards(moveForward.y, storedPos.y, (zoomSpeed * 0.01f) * Time.deltaTime);
            }
         

            mainCamera.transform.position = moveForward;

            // 거의 복귀했으면 FOV 조정 가능하게
            if (Vector3.Distance(mainCamera.transform.position, storedPos) < 0.01f)
            {
                isPositionStored = false;
                targetZoom = 74; 
            }
        }

        // 3) FOV 보간 적용
        mainCamera.fieldOfView = Mathf.Lerp(
            mainCamera.fieldOfView, 
            targetZoom, 
            Time.deltaTime * (zoomLerpSpeed * 0.5f)
        );
    }


    void HandleFreeCameraMovement()
    {
        float moveSpeed = 50f;
        float rotateSpeed = 700f;

        if (currentView == "Top Down")
        {
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

                // 예: 수평으로는 Y축, 수직으로는 X축 회전
                mainCamera.transform.Rotate(Vector3.up,   mouseX, Space.World);
                mainCamera.transform.Rotate(Vector3.right, -mouseY, Space.Self);
            }
            if (Input.GetMouseButton(2)) // 마우스 중간 버튼
            {
                float mouseX = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

                // 카메라의 위치를 업데이트 (패닝)
                mainCamera.transform.Translate(-mouseX, -mouseY, 0);
            }
        }
        else
        {
        if (Input.GetMouseButton(1)) // 마우스 오른쪽 버튼
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

            // 현재 카메라의 회전 값을 가져오기
            Vector3 currentRotation = mainCamera.transform.eulerAngles;

            // Y축만 업데이트 (수평 회전)
            currentRotation.y += mouseX;
            currentRotation.x -= mouseY;


            // 업데이트된 회전을 적용
            mainCamera.transform.eulerAngles = currentRotation;
        }

        if (Input.GetMouseButton(2)) // 마우스 중간 버튼
        {
            float mouseX = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            // 카메라의 위치를 업데이트 (패닝)
            mainCamera.transform.Translate(-mouseX, -mouseY, 0);
        }
        }
    }


    public string GetCurrentView()
    {
        return currentView;  // 현재 뷰 상태 반환
    }
    void ResetCameraToHome()
    {
        // 로봇 기준으로 카메라 위치 계산
        if (robotTransform != null)
        {
            Vector3 offset = -robotTransform.forward * distanceBehindRobot + Vector3.up * heightAboveRobot;
            Vector3 targetPosition = robotTransform.position + offset;

            mainCamera.transform.position = targetPosition;
            mainCamera.transform.LookAt(robotTransform); // 로봇을 바라보도록 설정
            currentView = "Front";
            UpdateViewStatus();
        }
    }
    void OnCancelButtonClick()
    {
        panel.SetActive(false);
    }

    void OnStartButtonclick()
    {
        panel.SetActive(true);
    }

    // 기존 회전값에 추가 회전 및 위치 오프셋을 적용하는 함수
    void SetCameraView(Vector3 rotationAngles, Vector3 positionOffset, string viewName)
    {
        currentView = viewName;
        currentPositionOffset = positionOffset;
        currentRotationOffset = rotationAngles;

        isCameraFree = false; // 카메라가 다시 목표 위치로 이동하도록 설정
        UpdateViewStatus();
    }

    void SetTopDownView()
    {
        // 위에서 내려다보는 시점
        Vector3 rotationAngles = new Vector3(0, 0, 270); 
        Vector3 positionOffset = new Vector3(0, 18, 0);
        SetCameraView(rotationAngles, positionOffset, "Top Down");
        panel.SetActive(false);
    }

    void SetBackView()
    {
        // 기본 시점(Front)
        Vector3 rotationAngles = new Vector3(0, 0, 0);
        Vector3 positionOffset = new Vector3(0.26f, 1.58f, -4.71f);  // 원래 front 값
        SetCameraView(rotationAngles, positionOffset, "Front");
        panel.SetActive(false);
    }

    void SetLeftView()
    {
        Vector3 rotationAngles = new Vector3(0, 0, 0);
        Vector3 positionOffset = new Vector3(8, 1.58f, 0);
        SetCameraView(rotationAngles, positionOffset, "Left");
        panel.SetActive(false);
    }

    void SetRightView()
    {
        Vector3 rotationAngles = new Vector3(0, 0, 0);
        Vector3 positionOffset = new Vector3(-8, 1.58f, 0);
        SetCameraView(rotationAngles, positionOffset, "Right");
        panel.SetActive(false);
    }

    void UpdateViewStatus()
    {
        viewStatusText.text = "Unity View Mode: " + currentView;
    }
}