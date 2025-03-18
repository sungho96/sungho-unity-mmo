using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class navigation : MonoBehaviour
{
    ROSConnection ros;

    // 큐브 오브젝트를 목표 지점으로 사용
    public GameObject targetMarker;
    public string targetPositionTopic = "/unity/cmd"; // 목표 지점의 ROS2 토픽
    private Vector3 targetPositionFromROS;

    void Start()
    {
        // ROS2와의 연결 설정
        ros = ROSConnection.GetOrCreateInstance();

        // 타겟 위치 데이터 수신 구독
        ros.Subscribe<StringMsg>(targetPositionTopic, UpdateTargetPosition);
    }

    void Update()
    {
        // 타겟 위치를 업데이트
        if (targetMarker != null)
        {
            targetMarker.transform.position = targetPositionFromROS;
        }
    }

    // 타겟 위치 데이터를 처리하는 함수
    void UpdateTargetPosition(StringMsg msg)
    {
        // 메시지를 ; 기준으로 분리
        string[] data = msg.data.Split(';');
        
        if (data.Length >= 5)
        {
            string direction = data[0];
            float distanceValue = float.Parse(data[1]);
            float angleDegreeValue = float.Parse(data[3]);

            Debug.Log($"Received data - Direction: {direction}, Distance: {distanceValue}, Angle: {angleDegreeValue}");

            // 방향에 따른 처리
            switch (direction)
            {
                case "go_forward":
                    targetPositionFromROS = new Vector3(0, 0, distanceValue);
                    Debug.Log("Direction: go_forward, Moving target to Z position: " + distanceValue);
                    break;
                case "go_backward":
                    targetPositionFromROS = new Vector3(0, 0, -distanceValue);
                    Debug.Log("Direction: go_backward, Moving target to Z position: " + -distanceValue);
                    break;
                case "right_forward":
                    float radianRF = angleDegreeValue * Mathf.Deg2Rad;
                    targetPositionFromROS = new Vector3(distanceValue * Mathf.Sin(radianRF), 0, distanceValue * Mathf.Cos(radianRF));
                    Debug.Log("Direction: right_forward, Moving target to position: " + targetPositionFromROS);
                    break;
                case "right_backward":
                    float radianRB = angleDegreeValue * Mathf.Deg2Rad;
                    targetPositionFromROS = new Vector3(distanceValue * Mathf.Sin(radianRB), 0, -distanceValue * Mathf.Cos(radianRB));
                    Debug.Log("Direction: right_backward, Moving target to position: " + targetPositionFromROS);
                    break;
                case "left_forward":
                    float radianLF = angleDegreeValue * Mathf.Deg2Rad;
                    targetPositionFromROS = new Vector3(-distanceValue * Mathf.Sin(radianLF), 0, distanceValue * Mathf.Cos(radianLF));
                    Debug.Log("Direction: left_forward, Moving target to position: " + targetPositionFromROS);
                    break;
                case "left_backward":
                    float radianLB = angleDegreeValue * Mathf.Deg2Rad;
                    targetPositionFromROS = new Vector3(-distanceValue * Mathf.Sin(radianLB), 0, -distanceValue * Mathf.Cos(radianLB));
                    Debug.Log("Direction: left_backward, Moving target to position: " + targetPositionFromROS);
                    break;
                default:
                    Debug.LogWarning($"Unknown direction: {direction}");
                    break;
            }
        }
    }
}
