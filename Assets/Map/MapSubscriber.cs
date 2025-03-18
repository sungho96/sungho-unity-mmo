using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using System.Collections.Generic;
using System;


public class MapSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public Material cubeMaterial;
    public string frontLidarTopic = "/front_lidar/scan";
    public string rearLidarTopic = "/rear_lidar/scan";
    public GameObject lidarPointPrefab;
    private const float Rad2Deg = 57.2958f;
    private const float Deg2Rad = 0.0174533f;

    private List<GameObject> frontLidarPointObjects = new List<GameObject>();
    private List<GameObject> rearLidarPointObjects = new List<GameObject>();

    public List<Vector3> frontLidarPoints = new List<Vector3>();
    public List<Vector3> rearLidarPoints = new List<Vector3>();

    private int lidarPointLimit = 1000;
    private GameObject frontLidarParent;
    private GameObject rearLidarParent;

    private ComputeBuffer positionBuffer;
    private Vector3[] positions;
    private int numCubes =1000;

    private Vector3 storedPosition;
    private Vector3 storedEulerAngles;
    private bool isProcessingEnabled = true;  
    private bool isInitialValueSet = false;   
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<LaserScanMsg>(frontLidarTopic, ProcessFrontLidarData);
        ros.Subscribe<LaserScanMsg>(rearLidarTopic, ProcessRearLidarData);
        ros.Subscribe<StringMsg>("/fastlio_odom", FastLioOdomCallback);
        //ros.Subscribe<StringMsg>("/unity/cmd", DirectionCallback);

        frontLidarParent = new GameObject("FrontLidarParent");
        rearLidarParent = new GameObject("RearLidarParent");
        cubeMaterial.enableInstancing = true;

        // Pre-create a pool of LiDAR point objects for reuse
        for (int i = 0; i < lidarPointLimit; i++)
        {
            GameObject pointObject = Instantiate(lidarPointPrefab);
            pointObject.SetActive(false);
            pointObject.transform.SetParent(frontLidarParent.transform);
            frontLidarPointObjects.Add(pointObject);

            pointObject = Instantiate(lidarPointPrefab);
            pointObject.SetActive(false);
            pointObject.transform.SetParent(rearLidarParent.transform);
            rearLidarPointObjects.Add(pointObject);
        }
        positions = new Vector3[numCubes];
        for (int i = 0; i < numCubes; i++)
        {
            positions[i] =new Vector3(UnityEngine.Random.Range(-10.0f,10.0f),0, UnityEngine.Random.Range(-10.0f,10.0f));
        }

        positionBuffer = new ComputeBuffer(numCubes, sizeof(float)*3);
        positionBuffer.SetData(positions);
        cubeMaterial.SetBuffer("positionBuffer",positionBuffer);
        
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

        if (isProcessingEnabled)
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
            eulerAngles.y *= -1;
            eulerAngles.x = 0;
            
            float x = float.Parse(parts[1]) * -1;
            float y = 0;
            float z = float.Parse(parts[0]);

            Vector3 pos = new Vector3(x, y, z);
            storedPosition = new Vector3(x, y, z);
            storedEulerAngles = eulerAngles;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }
    }
    void ProcessFrontLidarData(LaserScanMsg msg)
    {
        Update_Front_LidarPoints(msg, frontLidarPoints, frontLidarPointObjects, frontLidarParent.transform, Color.gray);
    }

    void ProcessRearLidarData(LaserScanMsg msg)
    {
        Update_Rear_LidarPoints(msg, rearLidarPoints, rearLidarPointObjects, rearLidarParent.transform, Color.gray);
    }

    void Update_Front_LidarPoints(LaserScanMsg msg, List<Vector3> lidarPoints, List<GameObject> lidarPointObjects, Transform parentTransform, Color color)
    {
        lidarPoints.Clear();
        float angle = msg.angle_min;
        for (int i = 0; i < msg.ranges.Length; i++)
        {
            float range = msg.ranges[i];
            if (range < msg.range_min || range > msg.range_max)
            {
                angle += msg.angle_increment;
                continue;
            }
            float x = range * Mathf.Cos(angle);
            float y = range * Mathf.Sin(angle);
            float x_1 = x * Mathf.Cos(-0.7854f)- y *Mathf.Sin(-0.7854f);
            float y_1 = x *Mathf.Sin(-0.7854f) + y * Mathf.Cos(-0.7854f);
            Vector3 point = new Vector3(-x_1-0.267f, 0, -y_1+0.402f);
            
            float storedRadian = storedEulerAngles.y * Deg2Rad*-1;
            float x_2 = point.x * Mathf.Cos(storedRadian)- point.z *Mathf.Sin(storedRadian);
            float y_2 = point.x *Mathf.Sin(storedRadian) + point.z * Mathf.Cos(storedRadian);
            Vector3 point_2 = new Vector3(x_2, 0,y_2);

            lidarPoints.Add(point_2);
            angle += msg.angle_increment;
        }
        

        for (int i = 0; i < lidarPointObjects.Count; i++)
        {
            if (i < lidarPoints.Count)
            {
                lidarPointObjects[i].SetActive(true);
                lidarPointObjects[i].transform.position = parentTransform.position + lidarPoints[i] + storedPosition;
                //lidarPointObjects[i].transform.eulerAngles = parentTransform.eulerAngles + storedEulerAngles;
                lidarPointObjects[i].GetComponent<Renderer>().material.color = color;
            }
            else
            {
                lidarPointObjects[i].SetActive(false);
            }
        }
    }
    void Update_Rear_LidarPoints(LaserScanMsg msg, List<Vector3> lidarPoints, List<GameObject> lidarPointObjects, Transform parentTransform, Color color)
    {
        lidarPoints.Clear();
        float angle = msg.angle_min;
        for (int i = 0; i < msg.ranges.Length; i++)
        {
            float range = msg.ranges[i];
            if (range < msg.range_min || range > msg.range_max)
            {
                angle += msg.angle_increment;
                continue;
            }
            float x = range * Mathf.Cos(angle);
            float y = range * Mathf.Sin(angle);
            float x_1 = x * Mathf.Cos(-3.9357f)- y *Mathf.Sin(-3.9357f);
            float y_1 = x *Mathf.Sin(-3.9357f) + y * Mathf.Cos(-3.9357f);
            Vector3 point = new Vector3(-x_1+0.267f, 0, -y_1-0.402f);

            float storedRadian = storedEulerAngles.y * Deg2Rad*-1;
            float x_2 = point.x * Mathf.Cos(storedRadian)- point.z *Mathf.Sin(storedRadian);
            float y_2 = point.x *Mathf.Sin(storedRadian) + point.z * Mathf.Cos(storedRadian);
            Vector3 point_2 = new Vector3(x_2,0, y_2);

            lidarPoints.Add(point_2);
            angle += msg.angle_increment;
        }
        

        for (int i = 0; i < lidarPointObjects.Count; i++)
        {
            if (i < lidarPoints.Count)
            {
                lidarPointObjects[i].SetActive(true);
                lidarPointObjects[i].transform.position = parentTransform.position + lidarPoints[i] + storedPosition;
                //lidarPointObjects[i].transform.eulerAngles = parentTransform.eulerAngles + storedEulerAngles;
                lidarPointObjects[i].GetComponent<Renderer>().material.color = color;
            }
            else
            {
                lidarPointObjects[i].SetActive(false);
            }
        }
    }
}

