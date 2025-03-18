using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System;

public class CombinedVisualizer : MonoBehaviour
{
    public ROSConnection ros;
    public GameObject conePrefab;
    public float pointLifetime = 5.0f;
    private const int POINTS_LIMIT = 10000;
    private GameObject[] pointCloudObjects;
    private List<GameObject> clonedPointCloudObjects = new List<GameObject>();
    private List<GameObject> pointCloudParents = new List<GameObject>();
    private List<LineRenderer> clonedLineRenderers = new List<LineRenderer>();
    private GameObject pointCloudParent;
    private GameObject fixedPointCloudParent;
    private GameObject livePathParent;
    private bool holdLastPoints = false;
    private List<Coroutine> destroyCoroutines = new List<Coroutine>();
    private Coroutine destroyPointsCoroutine;
    private List<Vector3> line_points;

    private LineRenderer lineRenderer;
    private List<GameObject> pointObjects = new List<GameObject>();
    //private bool made = false;
    private string currentViewMode = "frontview";

    // Add events for broadcasting data
    public static event Action<float, float, float> OnPositionUpdate;
    public static event Action<float> OnRotationUpdate;
    public Vector3 storedPosition;
    public Vector3 storedEulerAngles;
    private Vector3 savedPosition;       // Path 오브젝트의 이전 위치 저장
    private Vector3 savedEulerAngles;   // Path 오브젝트의 이전 회전 저장
    private bool isProcessingEnabled = false;  
    private bool isInitialValueSet = false;   
    private const float Deg2Rad = 0.0174533f;
    // private bool isFirstExecution = true; 
    // private bool firstPointCloudReceived = false; 



    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROSConnection component not found on the GameObject.");
            return;
        }

        //ros.Subscribe<PointCloud2Msg>("/closest_pointcloud_topic", PointCloudCallback);
        ros.Subscribe<StringMsg>("/fastlio_odom", FastLioOdomCallback);
        ros.Subscribe<StringMsg>("/trajectory", PathCallback);
        ros.Subscribe<StringMsg>("/view_mode",ViewCallback);
        ros.Subscribe<StringMsg>("/unity/cmd", DirectionCallback);


        pointCloudParent = new GameObject("PointCloudParent");
        fixedPointCloudParent = new GameObject("FixedPointCloudParent");
        livePathParent = new GameObject("LivePathParent");

        pointCloudObjects = new GameObject[POINTS_LIMIT];
        for (int i = 0; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointCloudObjects[i].transform.localScale = new Vector3(0.025f, 0.025f, 0.05f);
            pointCloudObjects[i].SetActive(false);
            pointCloudObjects[i].transform.parent = pointCloudParent.transform;
        }

        lineRenderer = livePathParent.AddComponent<LineRenderer>();
        Material lineMaterial = new Material(Shader.Find("Unlit/Color"));
        lineMaterial.color = Color.yellow;
        lineMaterial.renderQueue = 1000;
        lineMaterial.SetInt("_ZWrite", 0); // 깊이 쓰기 비활성화
        lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = false;

        fixedPointCloudParent.transform.position = new Vector3(0, 0, 0);
        livePathParent.transform.position = new Vector3(0, 0, 0);

        ResetStateEvent.OnResetStateChanged += HandleResetStateChanged; // **리셋 상태 변경 이벤트 구독**
    }
    void OnDestroy()
{
    // Reset 이벤트 구독 해제
    ResetStateEvent.OnResetStateChanged -= HandleResetStateChanged; // **구독 해제**
}


    void Update()
    {
        // D 키를 누르면 holdLastPoints를 토글하여 큐브를 수동으로 활성화/비활성화
        if (Input.GetKeyDown(KeyCode.D))
        {
            holdLastPoints = !holdLastPoints;
            if (holdLastPoints)
            {
                Debug.Log("Point cloud data reception paused.");
                DisableAllPoints();
            }
            else
            {
                Debug.Log("Point cloud data reception resumed.");
                lineRenderer.enabled = true;
            }
        }

        // isProcessingEnabled가 true일 때 큐브 비활성화, false일 때 다시 활성화
        if (!holdLastPoints)
        {
            if(isProcessingEnabled)
            {
            // Processing이 활성화된 상태이므로 큐브를 비활성화
            holdLastPoints = true;
            DisableAllPoints();  // 큐브 비활성화
            }
            
        else if (!isProcessingEnabled && holdLastPoints)
        {
            // Processing이 비활성화된 상태이므로 큐브를 다시 활성화
            holdLastPoints = false;
            lineRenderer.enabled = true;  // 큐브 활성화
        }
        }

        // C 키: 포인트 클라우드 복제
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClonePointCloudAndPath();
        }

        // O 키: 복제된 클라우드 삭제
        if (Input.GetKeyDown(KeyCode.O))
        {
            DeleteLastPointCloudAndPath();
        }
    }

    

    void PointCloudCallback(PointCloud2Msg msg)
    {
        if (holdLastPoints)
        {
            return;
        }

        int totalPoints = Math.Min((int)msg.width * (int)msg.height, POINTS_LIMIT);

        for (int i = 0; i < totalPoints; i++)
        {
            int startIndex = i * (int)msg.point_step;
            if (startIndex + 12 >= msg.data.Length)
            {
                Debug.LogError($"Index out of range: startIndex {startIndex} exceeds data length {msg.data.Length}.");
                break;
            }

            float x = BitConverter.ToSingle(msg.data, startIndex);
            float y = BitConverter.ToSingle(msg.data, startIndex + 4);
            float z = BitConverter.ToSingle(msg.data, startIndex + 8);
            uint color = BitConverter.ToUInt32(msg.data, startIndex + 12);

            Color32 pointColor = new Color32(
                (byte)((color >> 16) & 0xFF),
                (byte)((color >> 8) & 0xFF),
                (byte)(color & 0xFF),
                (byte)((color >> 24) & 0xFF)
            );

            Vector3 pointPosition = new Vector3(-y, z, x);
            pointCloudObjects[i].SetActive(true);
            pointCloudObjects[i].transform.localPosition = pointPosition;
            pointCloudObjects[i].GetComponent<Renderer>().material.color = pointColor;
        }

        for (int i = totalPoints; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i].SetActive(false);
        }
        pointCloudParent.transform.position = storedPosition;
        pointCloudParent.transform.eulerAngles = storedEulerAngles;
        
       
    }

    void DirectionCallback(StringMsg message)
    {
        if (message.data == "start")
        {
           
            isProcessingEnabled = true;
        }
        else if (message.data == "completed")
        {
           
            isProcessingEnabled = false;
        }
    }
    void FastLioOdomCallback(StringMsg message)
    {
        if (!isInitialValueSet)
        {
            ProcessOdomMessage(message);
            isInitialValueSet = true;
            return;
        }

        if (!isProcessingEnabled)
        {
            ProcessOdomMessage(message);
        }
    }

    void ProcessOdomMessage(StringMsg message)
    {
        string[] parts = message.data.Split(';');

        if (parts.Length != 10)
        {
            Debug.LogError("Invalid message format for /fastlio_odom. Expected 7 values.");
            return;
        }

        try
        {
            Quaternion rot = new Quaternion(
                float.Parse(parts[4]) * -1,
                float.Parse(parts[5]),
                float.Parse(parts[3]),
                float.Parse(parts[6])
            );

            Vector3 eulerAngles = rot.eulerAngles;        
            OnRotationUpdate?.Invoke(eulerAngles.y);
            eulerAngles.y *= -1;
            eulerAngles.x = 0;
            
            float x = float.Parse(parts[1])*-1;
            float y = 0;
            float z = float.Parse(parts[0]);

            Vector3 pos = new Vector3(x, y, z);
            storedPosition = new Vector3(x, y, z);
            storedEulerAngles = eulerAngles;
            OnPositionUpdate?.Invoke(x, y, z);  

        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }
    }  
    void ViewCallback(StringMsg msg)
        {
            currentViewMode = msg.data;
            Debug.Log($"View mode set to: {currentViewMode}");
        }

    void PathCallback(StringMsg message)
    {
        if (holdLastPoints)
        {
            return;
        }
        
        line_points = new List<Vector3>();
        ClearAllPoints();

        string data = message.data.Trim(new char[] { '[', ']' });
        string[] sets = data.Split(new string[] { "], [" }, StringSplitOptions.None);
        int set_length = sets.Length;
        int cnt = 0;
        foreach (var set in sets)
        {
            string cleanedSet = set.Replace("(", "").Replace(")", "").Replace("]", "").Replace("[", "").Replace("'", "");

            string[] stringValues = cleanedSet.Split(',');
            List<float> floatValues = new List<float>();
            
            int point_length = stringValues.Length / 9;
            
            lineRenderer.positionCount = set_length * point_length;
                  

            foreach (var str in stringValues)
            {
                string trimmedStr = str.Trim();

                if (float.TryParse(trimmedStr, out float value))
                {
                    floatValues.Add(value);
                }
                else
                {
                    Debug.LogError($"Failed to parse '{trimmedStr}' to float.");
                    return;
                }
            }
            

            for (int i = 0; i < floatValues.Count; i += 9)
            {
                

                float x = -floatValues[i + 1];
                float y = floatValues[i + 2];
                float z = floatValues[i];
                Vector3 point =  new Vector3(x,y,z);
                float storedRadian = storedEulerAngles.y * Deg2Rad*-1;
                float x_2 = point.x * Mathf.Cos(storedRadian)- point.z *Mathf.Sin(storedRadian);
                float z_2 = point.x *Mathf.Sin(storedRadian) + point.z * Mathf.Cos(storedRadian);
                Vector3 pointPosition = new Vector3(x_2, y, z_2) + storedPosition;

                

                float rotX = -floatValues[i + 4];
                float rotY = floatValues[i + 5];
                float rotZ = floatValues[i + 3];
                switch (currentViewMode)
                {
                    case "frontview":
                        rotY -= 90.0f;  // Front view adjustment
                        break;
                    case "leftview":
                        rotX += 0.0f;  // Left view adjustment
                        break;
                    case "ceilingview":
                        rotX -= 90.0f;  // Ceiling view adjustment
                        break;
                    case "floorview":
                        rotX += 90.0f;  // Floor view adjustment
                        break;
                    default:
                        rotX += 0.0f;  // Default adjustment for front view
                        break;
                }
                Vector3 pointRotation = new Vector3(rotX, rotY, rotZ)+ storedEulerAngles;
                
                float x_3 = -floatValues[i + 7];
                float y_3 = floatValues[i + 8]; //+0.2f;
                float z_3 = floatValues[i + 6];//- 0.2f;
                switch (currentViewMode)
                {
                    case "frontview":
                        z_3 -= 0.1f;  // Front view adjustment
                        break;
                    case "leftview":
                        x_3 += 0.2f;  // Left view adjustment
                        break;
                    case "ceilingview":
                        y_3 -= 0.3f;  // Ceiling view adjustment
                        break;
                    case "floorview":
                        y_3 += 0.3f;  // Floor view adjustment
                        break;
                    default:
                        x_3 += 0.0f;  // Default adjustment for front view
                        break;
                }
                Vector3 point_1 =  new Vector3(x_3,y_3,z_3);
                float x_4 = point_1.x * Mathf.Cos(storedRadian)- point_1.z *Mathf.Sin(storedRadian);
                float z_4 = point_1.x *Mathf.Sin(storedRadian) + point_1.z * Mathf.Cos(storedRadian);
                Vector3 pointPosition_1 = new Vector3(x_4, y_3, z_4) + storedPosition;


                lineRenderer.SetPosition(cnt,pointPosition_1);   
                
                line_points.Add(pointPosition_1);
              
                CreateNewPoint(pointPosition, pointRotation);
                
                cnt ++;
                
            }
            ClonePointCloudAndPath();
        }
        // if (isFirstExecution)
        // {
        //     isFirstExecution = false; // 플래그 업데이트
        // }
        // else
        // {
        //     ClonePointCloudAndPath();
        // }
    }

    void CreateNewPoint(Vector3 position, Vector3 eulerAngles)
    {
        if (conePrefab == null)
        {
            Debug.LogError("Cone prefab is not assigned.");
            return;
        }

        GameObject pointObject = Instantiate(conePrefab, livePathParent.transform);
        pointObject.transform.localScale = new Vector3(0.025f, 0.1f, 0.025f);
        pointObject.transform.localPosition = position;
        pointObject.transform.localEulerAngles = eulerAngles;

        pointObjects.Add(pointObject);

        if (!holdLastPoints)
        {
            Coroutine destroyCoroutine = StartCoroutine(DestroyAfterTime(pointObject, pointLifetime));
            destroyCoroutines.Add(destroyCoroutine);
        }
    }

    void ClearAllPoints()
    {
        foreach (var pointObject in pointObjects)
        {
            Destroy(pointObject);
        }
        pointObjects.Clear();
    }

    IEnumerator DestroyAfterTime(GameObject pointObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pointObjects.Contains(pointObject) && !holdLastPoints)
        {
            int index = pointObjects.IndexOf(pointObject);
            pointObjects.Remove(pointObject);
            Destroy(pointObject);
        }
    }
    void ClonePointCloudAndPath()
{
    Debug.Log("C key pressed - Cloning Point Cloud and Path");

    // 포인트 클라우드 클론을 담을 새로운 부모 객체 생성
    GameObject newParent = new GameObject("PointCloudCloneParent");
    newParent.transform.parent = fixedPointCloudParent.transform;
    pointCloudParents.Add(newParent);

    for (int i = 0; i < POINTS_LIMIT; i++)
    {
        if (pointCloudObjects[i].activeSelf)
        {
            // 객체의 절대 위치와 회전을 설정
            Vector3 absolutePosition = pointCloudObjects[i].transform.position; // 절대 위치
            Quaternion absoluteRotation = pointCloudObjects[i].transform.rotation; // 절대 회전

            GameObject clone = Instantiate(pointCloudObjects[i], absolutePosition, absoluteRotation);
            clone.transform.localScale = pointCloudObjects[i].transform.localScale;
            clone.GetComponent<Renderer>().material.color = pointCloudObjects[i].GetComponent<Renderer>().material.color;
            clone.transform.parent = newParent.transform;
            clonedPointCloudObjects.Add(clone);
        }
    }

    GameObject pathParent = new GameObject("PathCloneParent");
    pathParent.transform.parent = fixedPointCloudParent.transform;
    pathParent.transform.localPosition = Vector3.zero;
    pathParent.transform.localRotation = Quaternion.identity;

    LineRenderer pathLineRenderer = pathParent.AddComponent<LineRenderer>();
    pathLineRenderer.useWorldSpace = false;
    pathLineRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.yellow };
    pathLineRenderer.startColor = Color.yellow;
    pathLineRenderer.endColor = Color.yellow;
    pathLineRenderer.startWidth = 0.02f;
    pathLineRenderer.endWidth = 0.02f;
    pathLineRenderer.positionCount = pointObjects.Count;

    int idx = 0;
    foreach (var pointObject in pointObjects)
    {
        GameObject clone = Instantiate(pointObject, pointObject.transform.position, pointObject.transform.rotation);
        clone.transform.localScale = pointObject.transform.localScale;
        clone.GetComponent<Renderer>().material.color = pointObject.GetComponent<Renderer>().material.color;
        clone.transform.parent = pathParent.transform;

        float rad_eulerY = fixedPointCloudParent.transform.eulerAngles.y * Mathf.PI / 180;

        Vector3 POS = line_points[idx] - fixedPointCloudParent.transform.position;
        float x_1 = POS.x * Mathf.Cos(rad_eulerY) - POS.z * Mathf.Sin(rad_eulerY);
        float z_1 = POS.x * Mathf.Sin(rad_eulerY) + POS.z * Mathf.Cos(rad_eulerY);

        Vector3 pos = new Vector3(x_1, POS.y, z_1);
            
        pathLineRenderer.SetPosition(idx, pos);
        idx ++;

        //Debug.Log($"Cloned point position: {clone.transform.localPosition}");
    }

    clonedLineRenderers.Add(pathLineRenderer);
}


void DeleteLastPointCloudAndPath()
    {
        if (pointCloudParents.Count > 0)
        {
            GameObject lastParent = pointCloudParents[pointCloudParents.Count - 1];
            pointCloudParents.RemoveAt(pointCloudParents.Count - 1);
            Destroy(lastParent);
            Debug.Log("Last cloned point cloud deleted.");
        }
        else
        {
            Debug.LogWarning("No cloned point clouds to delete.");
        }

        if (clonedLineRenderers.Count > 0)
        {
            LineRenderer lastLineRenderer = clonedLineRenderers[clonedLineRenderers.Count - 1];
            clonedLineRenderers.RemoveAt(clonedLineRenderers.Count - 1);
            Destroy(lastLineRenderer.gameObject);
            Debug.Log("Last cloned path deleted.");
        }
        else
        {
            Debug.LogWarning("No cloned paths to delete.");
        }
    }

    void DisableAllPoints()
    {
        for (int i = 0; i < POINTS_LIMIT; i++)
        {
            pointCloudObjects[i].SetActive(false);
        }
        foreach (var pointObject in pointObjects)
        {
            pointObject.SetActive(false);
        }
        lineRenderer.enabled = false;
        Debug.Log("All points have been disabled.");

    }
    void HandleResetStateChanged(bool isActive)
    {
        Debug.Log($"[CombinedVisualizer] Reset state received: {isActive}");

        if (isActive)
        {
            // 리셋 상태로 전환: 현재 위치와 회전을 저장하고 초기화
            savedPosition = storedPosition; // 현재 위치 저장
            savedEulerAngles = storedEulerAngles; // 현재 회전 저장

            storedPosition = Vector3.zero; // 초기화
            storedEulerAngles = Vector3.zero;

            foreach (var pointObject in pointObjects)
            {
                pointObject.transform.localPosition = Vector3.zero;
                pointObject.transform.localEulerAngles = Vector3.zero;
            }
            Debug.Log("[CombinedVisualizer] Path objects reset to zero.");
        }
        else
        {
            // 리셋 해제: 저장된 위치와 회전으로 복구
            storedPosition = savedPosition;
            storedEulerAngles = savedEulerAngles;

            foreach (var pointObject in pointObjects)
            {
                pointObject.transform.localPosition = savedPosition;
                pointObject.transform.localEulerAngles = savedEulerAngles;
            }
            Debug.Log("[CombinedVisualizer] Path objects restored to previous position and rotation.");
        }
    }
}