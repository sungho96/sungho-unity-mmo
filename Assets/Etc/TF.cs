using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Tf2;  // tf2_msgs에 해당하는 네임스페이스 사용
using RosMessageTypes.Geometry;

public class TFCameraController : MonoBehaviour
{
    ROSConnection ros;
    public string tfTopic = "/tf";  // TF 데이터를 수신하는 ROS 토픽
    public string tfTopic_static = "/tf_static"; 
    public Camera targetCamera;     // Unity의 카메라 오브젝트
    public string parentFrame = "odom";  // 부모 프레임
    public string childFrame = "pan_link"; // 자식 프레임

    private TransformStampedMsg odomToBaseLink = null;
    private TransformStampedMsg baseLinkToPanLink = null;

    void Start()
    {
        // ROS 연결 설정
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<RosMessageTypes.Tf2.TFMessageMsg>(tfTopic, OnTFReceived);  // tf2_msgs로 구독
        ros.Subscribe<RosMessageTypes.Tf2.TFMessageMsg>(tfTopic_static, OnTFReceived);  // tf2_msgs로 구독
    }

    void OnTFReceived(TFMessageMsg tfMessage)
    {
        // 최신 데이터를 갱신하기 위해 매번 새로운 데이터를 갱신할 수 있도록 수정
        foreach (TransformStampedMsg tf in tfMessage.transforms)
        {   
            string frameId = tf.header.frame_id;
            string childFrameId = tf.child_frame_id;

            // 최신 변환 데이터를 계속 갱신
            if (frameId == "odom" && childFrameId == "base_link")
            {
                odomToBaseLink = tf;  // 항상 최신 데이터로 갱신
            }
            else if (frameId == "base_link" && childFrameId == "pan_link")
            {
                baseLinkToPanLink = tf;  // 항상 최신 데이터로 갱신
            }
        }

        // 두 변환이 모두 있을 때만 처리
        if (odomToBaseLink != null && baseLinkToPanLink != null)
        {
            // odom -> base_link와 base_link -> pan_link의 변환을 합쳐 최종 위치 및 회전 계산
            Vector3 position = CombinePositions(odomToBaseLink.transform.translation, baseLinkToPanLink.transform.translation);
            Quaternion rotation = CombineRotations(odomToBaseLink.transform.rotation, baseLinkToPanLink.transform.rotation);
            Vector3 eulerRotation = rotation.eulerAngles;
            eulerRotation.y = -eulerRotation.y;  // Y축 반전

            // Unity 카메라에 변환 적용
            targetCamera.transform.position = position;
            targetCamera.transform.rotation = Quaternion.Euler(eulerRotation);
            //Debug.Log("Camera position and rotation updated from odom -> base_link -> pan_link.");
        }
        else
        {
            Debug.Log("Transform data is missing.");
        }
    }

    // 위치 합산 함수
    Vector3 CombinePositions(Vector3Msg t1, Vector3Msg t2)
    {
        return new Vector3(
             (float)(t1.y+ t2.y)*-1,
            (float)(t1.z + t2.z),
            (float)(t1.x + t2.x)
        );
    }

    // 회전 합산 함수 (쿼터니언 곱)
    Quaternion CombineRotations(QuaternionMsg r1, QuaternionMsg r2)
    {
        Quaternion q1 = new Quaternion((float)r1.y*-1, (float)r1.z, (float)r1.x, (float)r1.w);
        Quaternion q2 = new Quaternion((float)r2.y*-1, (float)r2.z, (float)r2.x, (float)r2.w);
        Debug.Log($"q1*q2={q1*q2}");
        return q1 * q2;
    }
}
