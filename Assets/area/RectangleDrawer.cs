using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class CubeSpawner : MonoBehaviour
{
    public GameObject markerPrefab;
    public Material lineMaterial;
    public GameObject textPrefab;
    public Material outlineMaterial;
    public CameraTransformChanger cameraTransformChanger;

    // UI 요소들
    public Button areaControlButton;    // Area_Control_Button
    public GameObject controlPanel;     // ControlPanel 패널
    public Button deleteButton;         // Delete 버튼
    public Button undoButton;           // Undo 버튼
    public Button cancelButton;         // Cancel 버튼
    public Button toggleButton;         // toggle 버튼
    public Button TaskSendButton;
    public Button copyButton; // Copy 버튼 추가
    public GameObject boundingCubePrefab;
    private GameObject currentBoundingCube;
    private GameObject selectedCube;       // 선택된 큐브
    private Stack<GameObject> deletedGroups = new Stack<GameObject>(); // 삭제된 그룹들

    private int clickCount = 0;
    private Vector3 firstPosition;
    private Vector3 secondPosition;
    private Vector3 thirdPosition;

    private GameObject areaParent;
    private GameObject currentCubeGroupParent;
    private GameObject lastCube;
    private GameObject firstCubeInGroup;
    private string currentView;

    private TextMeshProUGUI buttonText;
    private bool isFeatureActive = false;

    private ROSConnection ros;
    private List<GameObject> spawnedCubes = new List<GameObject>();

    // ★ Undo용 상태를 저장할 클래스(혹은 struct) 정의
    [System.Serializable]
    private class UndoState
    {
        // 이번에 "한 번의 클릭"으로 생성된 모든 큐브들
        public List<GameObject> createdCubes;

        // 생성하기 직전의 상태
        public int oldClickCount;
        public GameObject oldLastCube;
        public GameObject oldFirstCubeInGroup;
        public GameObject oldCurrentCubeGroupParent;
    }

    // Undo할 때 쓸 스택
    private Stack<UndoState> undoStack = new Stack<UndoState>();


    void Start()
    {
        // ROS 통신 예시
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>("unity/cmd");

        // 부모 오브젝트(AreaParent)가 없으면 생성
        if (areaParent == null)
        {
            areaParent = new GameObject("AreaParent");
        }
        CreateNewCubeGroupParent();

        // 패널을 초기 비활성화
        controlPanel.SetActive(false);

        // 버튼 이벤트 연결
        areaControlButton.onClick.AddListener(OnAreaControlButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        undoButton.onClick.AddListener(OnUndoButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        TaskSendButton.onClick.AddListener(SendPositionData);
        copyButton.onClick.AddListener(OnCopyButtonClicked);

        // 버튼 초기 상태
        deleteButton.interactable = false;
        undoButton.interactable = false;

        buttonText = toggleButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "Unactive";
        toggleButton.onClick.AddListener(ToggleFeature);

        UpdateButtonStates();
    }

    void Update()
    {
        currentView = cameraTransformChanger.GetCurrentView();

        if (isFeatureActive)
        {
            // 마우스 왼쪽 클릭
            if (!IsPointerOverUI() && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // 혹시 이미 만들어진 큐브를 클릭한 경우 → 선택/해제 처리
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedCube = hit.collider.gameObject;
                    CubeSelectable selectable = clickedCube.GetComponent<CubeSelectable>();
                    if (selectable != null)
                    {
                        ActivateCube(clickedCube);
                        return; // 큐브 클릭 시에는 새로 생성 X
                    }
                }

                // **새로 큐브 생성** 로직
                if (currentView == "Top Down")
                {
                    HandleTopDownViewClick();
                }
                else if (currentView == "Front")
                {
                    HandleFrontViewClick();
                }
                else if (currentView == "Right" || currentView == "Left")
                {
                    HandleSideViewClick();
                }
            }
        }

        UpdateButtonStates();
    }

    // ★ Undo Stack에 기록
    //   - oldClickCount, oldLastCube 등을 함께 기록
    private void PushUndoState(List<GameObject> newlyCreated,
                               int oldClickCount, 
                               GameObject oldLastCube,
                               GameObject oldFirstCubeInGroup)
    {
        UndoState state = new UndoState
        {
            createdCubes = newlyCreated,
            oldClickCount = oldClickCount,
            oldLastCube = oldLastCube,
            oldFirstCubeInGroup = oldFirstCubeInGroup,
            oldCurrentCubeGroupParent = currentCubeGroupParent
        };
        undoStack.Push(state);
    }

    // Undo 버튼
    void OnUndoButtonClicked()
    {
        // 스택이 비었으면 종료
        if (undoStack.Count == 0)
        {
            Debug.Log("No more actions to undo!");
            return;
        }

        // 마지막 상태를 꺼내서(pop) → 복원
        UndoState state = undoStack.Pop();

        // 1) 생성되었던 큐브들을 모두 삭제
        if (state.createdCubes != null)
        {
            foreach (var cube in state.createdCubes)
            {
                RemoveCubeAndConnections(cube);
                spawnedCubes.Remove(cube);
            }
        }

        // 2) 원래 상태로 복원
        clickCount = state.oldClickCount;
        lastCube = state.oldLastCube;
        firstCubeInGroup = state.oldFirstCubeInGroup;
        currentCubeGroupParent = state.oldCurrentCubeGroupParent;

        Debug.Log($"[Undo] Restored to clickCount={clickCount}, lastCube={lastCube}, firstCubeInGroup={firstCubeInGroup}");

        UpdateButtonStates();
        UpdateBoundingCube(currentCubeGroupParent);
    }

    // 큐브 삭제 + 연결된 라인/텍스트 정리
    void RemoveCubeAndConnections(GameObject cube)
    {
        if (cube == null) return;

        CubeDraggable draggable = cube.GetComponent<CubeDraggable>();
        if (draggable != null)
        {
            foreach (var lineInfo in draggable.connectedLines)
            {
                if (lineInfo.lineRenderer != null)
                {
                    Destroy(lineInfo.lineRenderer.gameObject);
                }
                if (lineInfo.textMesh != null)
                {
                    Destroy(lineInfo.textMesh.gameObject);
                }
            }
        }

        Destroy(cube);
    }

    // 버튼 상태 갱신
    void UpdateButtonStates()
    {
        // Delete 버튼: 선택된 큐브가 있을 때만 활성화
        deleteButton.interactable = (selectedCube != null);

        // Undo 버튼: undoStack에 뭔가 들어있을 때만 활성화
        undoButton.interactable = (undoStack.Count > 0);
    }

    // AreaControl 열기
    void OnAreaControlButtonClicked()
    {
        controlPanel.SetActive(true);
        UpdateButtonStates();
    }

    // Delete 버튼
    void OnDeleteButtonClicked()
    {
        if (selectedCube != null)
        {
            DeleteCubeGroup(selectedCube);
            selectedCube = null; 
        }
    }

    // 그룹 전체 삭제
    void DeleteCubeGroup(GameObject cube)
    {
        GameObject parentGroup = cube.transform.parent.gameObject;

        // 필요시 스택에 저장
        deletedGroups.Push(parentGroup);

        // 영역 비활성화
        parentGroup.SetActive(false);

        // 클릭/참조 리셋
        selectedCube = null;
        clickCount = 0;
        lastCube = null;
        firstCubeInGroup = null;

        // 새 그룹
        CreateNewCubeGroupParent();
        UpdateButtonStates();
    }

    void OnCancelButtonClicked()
    {
        controlPanel.SetActive(false);
    }

    // UI 클릭 판정
    bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // 새 CubeGroup 생성
    void CreateNewCubeGroupParent()
    {
        currentCubeGroupParent = new GameObject("CubeGroup");
        currentCubeGroupParent.transform.parent = areaParent.transform;
        lastCube = null;
        firstCubeInGroup = null;
    }

    // 큐브 여러 개를 한꺼번에 생성 + Undo 기록
    //    → List<GameObject>를 반환해, 이 중 [1]이 "4번 점"임을 확인 가능
    List<GameObject> InstantiateCube(params Vector3[] positions)
    {
        // (1) 이번 클릭에 생성되는 새 큐브들
        List<GameObject> newCreated = new List<GameObject>();

        // (2) 생성 직전 상태
        int oldClickCount = clickCount;
        GameObject oldLastCube = lastCube;
        GameObject oldFirstCubeInGroup = firstCubeInGroup;

        // (3) 실제 생성
        foreach (var pos in positions)
        {
            GameObject cube = CreateSingleCube(pos);
            newCreated.Add(cube);
        }

        // (4) UndoState에 기록
        PushUndoState(newCreated, oldClickCount, oldLastCube, oldFirstCubeInGroup);

        // (5) 새로 만든 큐브 리스트 반환
        return newCreated;
    }

    // "큐브 1개" 실제 생성
    GameObject CreateSingleCube(Vector3 position)
    {
        GameObject cube = Instantiate(markerPrefab, position, Quaternion.identity);
        cube.name = "Cube";
        cube.transform.parent = currentCubeGroupParent.transform;
        cube.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        spawnedCubes.Add(cube);

        // 그룹의 첫 큐브라면
        if (firstCubeInGroup == null)
            firstCubeInGroup = cube;

        // CubeDraggable
        CubeDraggable draggable = cube.AddComponent<CubeDraggable>();
        draggable.SetCurrentView(currentView);

        // 만약 이전(lastCube)이 있다면 선+텍스트 연결
        if (lastCube != null)
        {
            LineRenderer line = DrawLineBetweenCubes(lastCube.transform.position, cube.transform.position);
            float distance = Vector3.Distance(lastCube.transform.position, cube.transform.position);
            Vector3 midPoint = (lastCube.transform.position + cube.transform.position) / 2;
            TextMesh text = DisplayDistanceText(midPoint, distance);

            line.transform.parent = currentCubeGroupParent.transform;
            text.transform.parent = currentCubeGroupParent.transform;

            CubeDraggable lastDraggable = lastCube.GetComponent<CubeDraggable>();
            CubeDraggable currentDraggable = cube.GetComponent<CubeDraggable>();

            ConnectedLineInfo lineInfoToCurrent = new ConnectedLineInfo
            {
                lineRenderer = line,
                textMesh = text,
                otherCube = currentDraggable
            };
            ConnectedLineInfo lineInfoToLast = new ConnectedLineInfo
            {
                lineRenderer = line,
                textMesh = text,
                otherCube = lastDraggable
            };
            lastDraggable.connectedLines.Add(lineInfoToCurrent);
            currentDraggable.connectedLines.Add(lineInfoToLast);
        }

        lastCube = cube;

        // CubeSelectable
        CubeSelectable selectable = cube.AddComponent<CubeSelectable>();
        selectable.outlineMaterial = outlineMaterial;

        return cube;
    }

    // ActivateCube (클릭으로 선택/해제)
    void ActivateCube(GameObject cube)
    {
        CubeSelectable selectable = cube.GetComponent<CubeSelectable>();
        if (selectable != null)
        {
            selectable.ToggleSelection();
            if (selectable.IsSelected)
            {
                if (selectedCube != null && selectedCube != cube)
                {
                    CubeSelectable prevSelectable = selectedCube.GetComponent<CubeSelectable>();
                    if (prevSelectable != null)
                        prevSelectable.Deselect();
                }
                selectedCube = cube;
            }
            else
            {
                selectedCube = null;
            }
        }
    }

    // LineRenderer 생성
    LineRenderer DrawLineBetweenCubes(Vector3 start, Vector3 end)
    {
        start.y += 0.01f;
        end.y += 0.01f;
        GameObject lineObj = new GameObject("LineBetweenObjects");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineMaterial.color = Color.white;

        lineRenderer.material = lineMaterial;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startWidth = 0.055f;
        lineRenderer.endWidth = 0.055f;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineObj.transform.parent = currentCubeGroupParent.transform;

        return lineRenderer;
    }

    // 거리 텍스트 표시
    TextMesh DisplayDistanceText(Vector3 position, float distance)
    {
        GameObject textObj = Instantiate(textPrefab, position, Quaternion.identity);
        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        textMesh.text = distance.ToString("F2") + "m";

        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;

        // 카메라 각도별로 텍스트 회전
        if (currentView == "Top Down")
        {
            textObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else if (currentView == "Back")
        {
            textObj.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currentView == "Right")
        {
            textObj.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (currentView == "Left")
        {
            textObj.transform.rotation = Quaternion.Euler(0, -90, 0);
        }

        textObj.transform.parent = currentCubeGroupParent.transform;
        return textMesh;
    }

    // TopDown 뷰 - 클릭 로직
    // 3번째 클릭 시 3,4번을 만들고 newCubes[1] (4번)을 1번과 연결
    void HandleTopDownViewClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xzPlane = new Plane(Vector3.up, 0);

        if (!xzPlane.Raycast(ray, out float enter))
            return;

        Vector3 clickPos = ray.GetPoint(enter);

        if (clickCount == 0)
        {
            firstPosition = clickPos;
            // 1번 점 생성
            InstantiateCube(firstPosition);
            clickCount++;
        }
        else if (clickCount == 1)
        {
            secondPosition = clickPos;
            // 2번 점 생성
            InstantiateCube(secondPosition);
            clickCount++;
        }
        else if (clickCount == 2)
        {
            // 3번,4번 점 생성
            Vector3 thirdClickPosition = clickPos;

            Vector3 side1Vector = secondPosition - firstPosition;
            Vector3 side1Direction = side1Vector.normalized;

            Vector3 clickVector = thirdClickPosition - secondPosition;
            Vector3 side2Direction = Vector3.Cross(side1Direction, Vector3.up).normalized;
            if (Vector3.Dot(clickVector, side2Direction) < 0)
                side2Direction = -side2Direction;

            float side2Length = Vector3.Project(clickVector, side2Direction).magnitude;

            thirdPosition = secondPosition + side2Direction * side2Length;
            Vector3 fourthPosition = firstPosition + side2Direction * side2Length;

            // 두 점 생성해서 리스트로 받음
            List<GameObject> newCubes = InstantiateCube(thirdPosition, fourthPosition);
            // newCubes[0]이 3번, [1]이 4번
            GameObject fourthCube = newCubes[1];

            // 1↔4 선 연결
            LineRenderer line = DrawLineBetweenCubes(fourthCube.transform.position, firstCubeInGroup.transform.position);
            float distance = Vector3.Distance(fourthCube.transform.position, firstCubeInGroup.transform.position);
            Vector3 midPoint = (fourthCube.transform.position + firstCubeInGroup.transform.position) / 2;
            TextMesh text = DisplayDistanceText(midPoint, distance);

            // 연결 정보
            CubeDraggable fourthDraggable = fourthCube.GetComponent<CubeDraggable>();
            CubeDraggable firstDraggable  = firstCubeInGroup.GetComponent<CubeDraggable>();

            ConnectedLineInfo lineInfoToFirst = new ConnectedLineInfo
            {
                lineRenderer = line,
                textMesh = text,
                otherCube = firstDraggable
            };
            ConnectedLineInfo lineInfoToFourth = new ConnectedLineInfo
            {
                lineRenderer = line,
                textMesh = text,
                otherCube = fourthDraggable
            };
            fourthDraggable.connectedLines.Add(lineInfoToFirst);
            firstDraggable.connectedLines.Add(lineInfoToFourth);
            UpdateBoundingCube(currentCubeGroupParent);


            // 초기화
            clickCount = 0;
            CreateNewCubeGroupParent();
        }
    }

    // Front 뷰 - 클릭 로직
    void HandleFrontViewClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (clickCount == 0)
        {
            Plane xzPlane = new Plane(Vector3.up, 0);
            if (xzPlane.Raycast(ray, out float enter))
            {
                firstPosition = ray.GetPoint(enter);
                InstantiateCube(firstPosition);  // 1번 점
                clickCount++;
            }
        }
        else if (clickCount == 1)
        {
            Plane xzPlane = new Plane(Vector3.up, 0);
            if (xzPlane.Raycast(ray, out float enter))
            {
                secondPosition = ray.GetPoint(enter);
                InstantiateCube(secondPosition); // 2번 점
                clickCount++;
            }
        }
        else if (clickCount == 2)
        {
            Plane cameraPlane = new Plane(-Camera.main.transform.forward, secondPosition);
            if (cameraPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                // 3번 점
                thirdPosition = new Vector3(secondPosition.x, hitPoint.y, secondPosition.z);
                // 4번 점
                Vector3 fourthPosition = new Vector3(firstPosition.x, thirdPosition.y, firstPosition.z);

                // 3,4번 생성
                List<GameObject> newCubes = InstantiateCube(thirdPosition, fourthPosition);
                GameObject fourthCube = newCubes[1]; // 4번

                // 1↔4 선 연결
                LineRenderer line = DrawLineBetweenCubes(fourthCube.transform.position, firstCubeInGroup.transform.position);
                float distance = Vector3.Distance(fourthCube.transform.position, firstCubeInGroup.transform.position);
                Vector3 midPoint = (fourthCube.transform.position + firstCubeInGroup.transform.position) / 2;
                TextMesh text = DisplayDistanceText(midPoint, distance);

                // 연결 정보
                CubeDraggable fourthDraggable = fourthCube.GetComponent<CubeDraggable>();
                CubeDraggable firstDraggable  = firstCubeInGroup.GetComponent<CubeDraggable>();

                ConnectedLineInfo lineInfoToFirst = new ConnectedLineInfo
                {
                    lineRenderer = line,
                    textMesh = text,
                    otherCube = firstDraggable
                };
                ConnectedLineInfo lineInfoToFourth = new ConnectedLineInfo
                {
                    lineRenderer = line,
                    textMesh = text,
                    otherCube = fourthDraggable
                };
                fourthDraggable.connectedLines.Add(lineInfoToFirst);
                firstDraggable.connectedLines.Add(lineInfoToFourth);
                UpdateBoundingCube(currentCubeGroupParent);


                // 초기화
                clickCount = 0;
                CreateNewCubeGroupParent();
            }
        }
    }

    // Side(Left/Right) 뷰 - 클릭 로직
    void HandleSideViewClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (clickCount == 0)
        {
            Plane xzPlane = new Plane(Vector3.up, 0);
            if (xzPlane.Raycast(ray, out float enter))
            {
                firstPosition = ray.GetPoint(enter);
                InstantiateCube(firstPosition); // 1번 점
                clickCount++;
            }
        }
        else if (clickCount == 1)
        {
            Plane xzPlane = new Plane(Vector3.up, 0);
            if (xzPlane.Raycast(ray, out float enter))
            {
                secondPosition = ray.GetPoint(enter);
                InstantiateCube(secondPosition); // 2번 점
                clickCount++;
            }
        }
        else if (clickCount == 2)
        {
            Plane cameraPlane = new Plane(-Camera.main.transform.forward, secondPosition);
            if (cameraPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                // 3번 점
                thirdPosition = new Vector3(secondPosition.x, hitPoint.y, secondPosition.z);
                // 4번 점
                Vector3 fourthPosition = new Vector3(firstPosition.x, thirdPosition.y, firstPosition.z);

                // 3,4번 생성
                List<GameObject> newCubes = InstantiateCube(thirdPosition, fourthPosition);
                GameObject fourthCube = newCubes[1]; // 4번

                // 1↔4 선 연결
                LineRenderer line = DrawLineBetweenCubes(fourthCube.transform.position, firstCubeInGroup.transform.position);
                float distance = Vector3.Distance(fourthCube.transform.position, firstCubeInGroup.transform.position);
                Vector3 midPoint = (fourthCube.transform.position + firstCubeInGroup.transform.position) / 2;
                TextMesh text = DisplayDistanceText(midPoint, distance);

                // 연결 정보
                CubeDraggable fourthDraggable = fourthCube.GetComponent<CubeDraggable>();
                CubeDraggable firstDraggable  = firstCubeInGroup.GetComponent<CubeDraggable>();

                ConnectedLineInfo lineInfoToFirst = new ConnectedLineInfo
                {
                    lineRenderer = line,
                    textMesh = text,
                    otherCube = firstDraggable
                };
                ConnectedLineInfo lineInfoToFourth = new ConnectedLineInfo
                {
                    lineRenderer = line,
                    textMesh = text,
                    otherCube = fourthDraggable
                };
                fourthDraggable.connectedLines.Add(lineInfoToFirst);
                firstDraggable.connectedLines.Add(lineInfoToFourth);
                UpdateBoundingCube(currentCubeGroupParent);


                // 초기화
                clickCount = 0;
                CreateNewCubeGroupParent();
            }
        }
    }

    // Toggle 기능 활성/비활성
    void ToggleFeature()
    {
        isFeatureActive = !isFeatureActive;
        buttonText.text = isFeatureActive ? "Active" : "Unactive";
    }

    // 위치 데이터 전송 버튼 예시

    void SendPositionData()
    {
        string positionDataString = GeneratePositionDataString();
        StringMsg message = new StringMsg(positionDataString);
        ros.Publish("unity/cmd", message);
        Debug.Log(positionDataString);
    }

    // (예시) 위치 데이터 문자열 생성
    string GeneratePositionDataString()
    {
        string data = "wall;";
        int groupIndex = 1;
        foreach (Transform cubeGroup in areaParent.transform)
        {
            if (!cubeGroup.gameObject.activeSelf)
                continue;

            List<Transform> cubes = new List<Transform>();
            foreach (Transform child in cubeGroup)
            {
                if (child.name.Contains("Cube"))
                    cubes.Add(child);
            }

            if (cubes.Count >= 3)
            {
                Vector3 startPos = cubes[0].position;
                Vector3 endPos = cubes[1].position;
                Vector3 heightPos = cubes[2].position;

                data += $"[{groupIndex},";
                data += $" ({startPos.x:F2}, {startPos.y:F2}, {startPos.z:F2}),";
                data += $" ({endPos.x:F2}, {endPos.y:F2}, {endPos.z:F2}),";
                data += $" ({heightPos.x:F2}, {heightPos.y:F2}, {heightPos.z:F2})],";

                groupIndex++;
            }
        }
        return data;
    }
    void OnCopyButtonClicked()
    {
        if (selectedCube == null) return;

        GameObject originalGroup = selectedCube.transform.parent.gameObject;
        Vector3 originalPosition = originalGroup.transform.position;
        Vector3 newPosition = originalPosition + new Vector3(0.2f, 0, 0.2f);

        // 새 그룹 생성
        GameObject newGroup = new GameObject("CubeGroup_Copy");
        newGroup.transform.parent = areaParent.transform;

        // markerPrefab의 기본 Material 확보
        Material defaultMat = markerPrefab.GetComponent<Renderer>().sharedMaterial;

        Dictionary<Transform, Transform> oldToNewCubeMap = new Dictionary<Transform, Transform>();

        // 1) 기존 Cube 복사
        foreach (Transform oldCube in originalGroup.transform)
        {
            if (!oldCube.name.Contains("Cube")) 
                continue; // Cube만 복사

            Vector3 cubeNewPos = oldCube.position + new Vector3(0.2f, 0, 0.2f);
            GameObject newCube = Instantiate(oldCube.gameObject, cubeNewPos, Quaternion.identity, newGroup.transform);
            newCube.name = "Cube_Copy";

            // **복사된 큐브가 '기본 색'으로 보이도록 Material 강제 세팅**
            Renderer newRend = newCube.GetComponent<Renderer>();
            if (newRend != null)
            {
                // 실시간 material이 아닌, 기본 material로 덮어쓰기
                newRend.material = defaultMat;  
            }

            // CubeSelectable 설정
            CubeSelectable newSelectable = newCube.GetComponent<CubeSelectable>();
            if (newSelectable == null)
            {
                newSelectable = newCube.AddComponent<CubeSelectable>();
                newSelectable.Initialize();
            }

            // **CubeSelectable의 originalMaterial 역시 기본 material로 설정**
            newSelectable.originalMaterial = defaultMat;

            // 복사된 큐브를 선택 해제 상태로
            newSelectable.Deselect();

            // CubeDraggable 설정
            CubeDraggable newDraggable = newCube.GetComponent<CubeDraggable>();
            if (newDraggable != null)
                newDraggable.SetCurrentView(currentView);

            oldToNewCubeMap[oldCube] = newCube.transform;
        }

        // 2) 라인(LineRenderer) 및 TextMesh 복사
        foreach (Transform oldCube in oldToNewCubeMap.Keys)
        {
            CubeDraggable oldDraggable = oldCube.GetComponent<CubeDraggable>();
            CubeDraggable newDraggable = oldToNewCubeMap[oldCube].GetComponent<CubeDraggable>();

            if (oldDraggable == null || newDraggable == null) 
                continue;

            foreach (ConnectedLineInfo oldLineInfo in oldDraggable.connectedLines)
            {
                // 연결된 반대쪽 Cube도 복사본이 있는 경우에만
                if (!oldToNewCubeMap.ContainsKey(oldLineInfo.otherCube.transform)) 
                    continue;

                Transform newOtherCube = oldToNewCubeMap[oldLineInfo.otherCube.transform];

                // 라인 생성
                LineRenderer newLine = DrawLineBetweenCubes(newDraggable.transform.position, newOtherCube.position);

                // 거리 텍스트 생성
                float distance = Vector3.Distance(newDraggable.transform.position, newOtherCube.position);
                Vector3 midPoint = (newDraggable.transform.position + newOtherCube.position) / 2;
                TextMesh newText = DisplayDistanceText(midPoint, distance);

                newLine.transform.parent = newGroup.transform;
                newText.transform.parent = newGroup.transform;

                // 양 큐브에 새 연결정보 추가
                CubeDraggable newOtherDraggable = newOtherCube.GetComponent<CubeDraggable>();

                ConnectedLineInfo newLineInfo1 = new ConnectedLineInfo
                {
                    lineRenderer = newLine,
                    textMesh = newText,
                    otherCube = newOtherDraggable
                };
                ConnectedLineInfo newLineInfo2 = new ConnectedLineInfo
                {
                    lineRenderer = newLine,
                    textMesh = newText,
                    otherCube = newDraggable
                };

                newDraggable.connectedLines.Add(newLineInfo1);
                newOtherDraggable.connectedLines.Add(newLineInfo2);
            }
        }

        Debug.Log("CubeGroup with lines copied, and forced to default color!");
    }
    void UpdateBoundingCube(GameObject cubeGroup)
    {
        List<Transform> cubePoints = new List<Transform>();
        foreach (Transform child in cubeGroup.transform)
        {
            if (child.name.StartsWith("Cube"))
                cubePoints.Add(child);
        }

        if (cubePoints.Count < 4)
        {
            Transform boundingCube = cubeGroup.transform.Find("BoundingCube(Clone)");
            if (boundingCube != null)
                boundingCube.gameObject.SetActive(false);
            return; 
        }

        Vector3 p1 = cubePoints[0].position;
        Vector3 p2 = cubePoints[1].position;
        Vector3 p3 = cubePoints[2].position;
        Vector3 p4 = cubePoints[3].position;

        Vector3 edge1 = p2 - p1;
        Vector3 edge2 = p4 - p1;

        float lengthX = edge1.magnitude;
        float lengthZ = edge2.magnitude;


        Vector3 center = p1 + edge1 * 0.5f + edge2 * 0.5f;


        Vector3 xAxis = edge1.normalized;
        Vector3 zAxis = edge2.normalized;
        Vector3 yAxis = Vector3.Cross(xAxis, zAxis).normalized;
        Quaternion orientation = Quaternion.LookRotation(zAxis, yAxis);
        Transform existingBC = cubeGroup.transform.Find("BoundingCube(Clone)");
        if (existingBC == null)
        {
            GameObject newBC = Instantiate(boundingCubePrefab);
            newBC.name = "AreaCube";
            newBC.tag = "BoundingCube";
            newBC.transform.SetParent(cubeGroup.transform, true);
            existingBC = newBC.transform;
        }
        existingBC.gameObject.SetActive(true);

        existingBC.position = center;
        existingBC.rotation = orientation;
        existingBC.localScale = new Vector3(lengthX, 0.15f, lengthZ);
       existingBC.Translate(0, lengthZ * 0.1f, 0, Space.Self);
    }

}
