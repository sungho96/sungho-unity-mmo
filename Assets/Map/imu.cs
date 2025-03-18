using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System;

public class ImuStringSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/unity/imu";
    public Vector3 imuOrientation;

    void Start()	
    {
        ros = ROSConnection.GetOrCreateInstance();
        if (ros == null)
        {
            Debug.LogError("ROSConnection instance is null");
            return;
        }
        ros.Subscribe<StringMsg>(topicName, ProcessIMUData);
    }

    void ProcessIMUData(StringMsg message)
    {
        if (message == null)
        {
            Debug.LogError("Received null message");
            return;
        }

        string[] dataValues = message.data.Split(';');
        // Debug.Log($"Split data: {string.Join(", ", dataValues)}");

        if (dataValues.Length == 6)
        {
            try
            {
                float roll = float.Parse(dataValues[0]);
                float pitch = float.Parse(dataValues[1]);
                float yaw = float.Parse(dataValues[2]);

                imuOrientation = new Vector3(roll, pitch, yaw);

               // Debug.Log($"Parsed Data - Roll: {roll}, Pitch: {pitch}, Yaw: {yaw}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occurred while parsing data: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Received data does not contain correct number of values");
        }
    }
}

