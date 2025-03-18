using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
//test
public class viewPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/robot_pose";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        
        // 퍼블리셔 등록
        ros.RegisterPublisher<StringMsg>(topicName);
        Debug.Log("Publisher registered for topic: " + topicName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PublishMessage("fk;J;[161.07,-6.48,-130.13,137.02,-71.32,88.07,30,30]");
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            PublishMessage("fk;J;[183.18,-11.34,-123.97,142.6,-4.54,81.16,30,30]");
        }
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
