using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class PaintPublisher : MonoBehaviour
{
    public enum CameraView
    {
        FrontView,
        LeftView
    }

    ROSConnection ros;
    public string topicName = "/robot_pose";

    public bool wall_mode = false;
    public CameraView cameraView;

    private List<Vector3> Data = new List<Vector3>();
    private List<float> tilt_angles = new List<float>();
    private List<float> angles = new List<float>();
    private List<float> yaws = new List<float>();
    private int repeat_count = 0;

    private bool isSubscribed = false;
    private bool dataReceived = false; // 데이터를 한 번만 받도록 제어

    private int setsLength = 0; 
    private int tupleCount = 0;  
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);
        Debug.Log("Publisher registered for topic: " + topicName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!isSubscribed)
            {
                ros.Subscribe<StringMsg>("/trajectory", Tracallback);
                isSubscribed = true;
                Debug.Log("Started receiving trajectory data.");
            }
            dataReceived = false; // 데이터를 다시 받을 수 있도록 설정
        }

        if (wall_mode == false && Input.GetKeyDown(KeyCode.P))
        {
            if (cameraView == CameraView.FrontView)
            {
                Debug.Log("Camera view is frontview, publishing front view data...");
                PublishFrontView();
            }
        }
    }

    void Tracallback(StringMsg message)
    {
        if (dataReceived) return; // 이미 데이터를 한 번 받았다면 반환

        try
        {
            string data = message.data.Trim(new char[] { '[', ']' });
            //1 len
            string[] sets = data.Split(new string[] { "], [" }, StringSplitOptions.None);
            setsLength = sets.Length; // sets 배열의 길이를 클래스 수준 변수에 저장
            Debug.Log(setsLength);
            Debug.Log("Sets content: " + sets[0]); // sets 배열의 내용을 출력
            foreach (var set in sets)
            {
                tupleCount = set.Split('(').Length - 1;
                Debug.Log("Number of tuples in set: " + tupleCount);
                // '(', ')', ']' 제거 후 공백 제거
                string cleanedSet = set.Replace("(", "").Replace(")", "").Replace("]", "").Replace("[", "").Replace("'", "");
                
                //string 2 len
                string[] stringValues = cleanedSet.Split(',');
                List<float> floatValues = new List<float>();

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

                // 7개씩 그룹으로 나누어 처리
                for (int i = 0; i < floatValues.Count; i += 7)
                {
                    if (i + 6 >= floatValues.Count)
                    {
                        Debug.LogError("Invalid data received");
                        return;
                    }

                    float x = floatValues[i];
                    float y = floatValues[i + 1];
                    float z = floatValues[i + 2];
                    float tilt_angle = floatValues[i + 4];
                    float angle = floatValues[i + 3];
                    float yaw = floatValues[i + 5];
                    repeat_count = (int)floatValues[i + 6]; // 마지막 값이 반복 횟수

                    float pose_x = y;
                    float pose_y = x * -1;
                    float pose_z = z;

                    Data.Add(new Vector3(pose_x, pose_y, pose_z));
                    tilt_angles.Add(tilt_angle);
                    angles.Add(angle);
                    yaws.Add(yaw);

                    Debug.Log($"Data received: {pose_x}, {pose_y}, {pose_z}, {tilt_angle}, {angle}, {yaw}, {repeat_count}");
                }
            }

            dataReceived = true; // 데이터가 한 번 받아졌음을 표시
            Debug.Log("Data received successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception in Tracallback: {ex}");
        }
    }

    async void PublishFrontView()
    {

        for (int i = 0; i < setsLength; i++)
        {
            for (int idx = 0; idx < tupleCount; idx++)
            {
                int dataIndex = i * setsLength + idx;
                string data;
                if (idx == 0)
                {
                    data = "ik;L;[" + (Data[dataIndex].x * 1000.0) + "," + (Data[dataIndex].y * 1000.0) + "," + (Data[dataIndex].z * 1000.0 - 985) + "," +
                           (-90 - angles[dataIndex]) + "," + tilt_angles[dataIndex] + "," + "0," + "1500," + "1500]";
                }
                else
                {
                    data = "iks;L;[" + (Data[dataIndex].x * 1000.0) + "," + (Data[dataIndex].y * 1000.0) + "," + (Data[dataIndex].z * 1000.0 - 985.0) + "," +
                           (-90 - angles[dataIndex]) + "," + tilt_angles[dataIndex] + "," + "0," + "800" + ",500]";
                }

                Debug.Log("Publishing front view data: " + data);
                PublishMessage(data);
                await Task.Delay(300);
            }

            //PublishMessage("iksr");
            Debug.Log("iksr");
        }

        Data.Clear();
        tilt_angles.Clear();
        angles.Clear();
        yaws.Clear();
    }

    void PublishMessage(string data)
    {
        StringMsg msg = new StringMsg
        {
            data = data
        };

        ros.Publish(topicName, msg);
        Debug.Log("Message published to topic: " + topicName + " with data: " + data);
    }
}
