using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class AutonomousTurn : MonoBehaviour
{
    public float rotationSpeed = 5f; // 회전 속도
    private float R_Yaw; // 현재 각도
    private bool isTurning = false; // 회전 중인지 여부
    private float goalAngle; // 목표 각도
    private int orient; // 회전 방향

    private ROSConnection ros; // ROS 연결 인스턴스
    public string rosTopic = "cmd_vel"; // ROS 토픽 이름

    // PID 제어기 변수
    public float Kp = 1.0f; // 비례 이득
    public float Ki = 0.0f; // 적분 이득
    public float Kd = 0.0f; // 미분 이득
    public float maxOutput = 5.0f; // PID 최대 출력
    public float minOutput = 0.1f; // PID 최소 출력
    private float previousError = 0.0f; // 이전 오차
    private float integral = 0.0f; // 적분 값
    private float previousTime; // 이전 시간

    public InputField goalAngleInput; // 목표 각도 입력 필드
    public Button startButton; // 시작 버튼


    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance(); // ROS 연결 인스턴스 생성 또는 가져오기
        ros.RegisterPublisher<TwistMsg>(rosTopic); 
        ros.Subscribe<StringMsg>("/fastlio_odom", FastLioOdomCallback); // ROS 토픽 퍼블리셔 등록
        previousTime = Time.time; // 초기 시간 설정

        startButton.onClick.AddListener(OnStartButtonClicked); // 시작 버튼 클릭 이벤트 리스너 등록
    }

    void FastLioOdomCallback(StringMsg message)
    {
        string[] parts = message.data.Split(';');

        if (parts.Length != 7)
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
            R_Yaw = eulerAngles.y; // R_Yaw 값 업데이트
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing position or rotation from /fastlio_odom: " + ex.Message);
        }
    }



    void OnStartButtonClicked()
    {
        if (isTurning) return; // 이미 회전 중이면 리턴

        // 목표 각도를 입력받음
        if (float.TryParse(goalAngleInput.text, out float goalDegree))
        {
            Debug.Log($"Start Turn from {R_Yaw} to {goalDegree} degrees"); // 로그 출력
            StartAutonomousTurn(goalDegree); // 회전 시작
        }
        else
        {
            Debug.LogError("Invalid input for goal angle"); // 유효하지 않은 입력 처리
        }
    }

    public void StartAutonomousTurn(float goalDegree)
    {
        if (isTurning) return; // 이미 회전 중이면 리턴

        Debug.Log("Start Autonomous Turn"); // 회전 시작 로그 출력
        isTurning = true; // 회전 중 상태로 설정
        StartCoroutine(AutonomousTurnCoroutine(goalDegree)); // 회전 코루틴 시작
    }

    private IEnumerator AutonomousTurnCoroutine(float goalDegree)
    {
        orient = goalDegree >= 0 ? 1 : -1; // 회전 방향 설정
        float startYaw = R_Yaw; // 초기 R_Yaw 값 저장
        goalAngle = startYaw + goalDegree; // 목표 각도 설정

        if (goalAngle < 0)
        {
            goalAngle += 360; // 목표 각도가 음수면 보정
        }
        else if (goalAngle >= 360)
        {
            goalAngle -= 360; // 목표 각도가 360도를 넘으면 보정
        }

        while (true)
        {
            float yaw = R_Yaw; // 현재 yaw 값 업데이트

            float error = goalAngle - yaw; // 오차 계산
            if (error < -180)
            {
                error += 360; // 오차가 -180도보다 작으면 보정
            }
            if (error > 180)
            {
                error -= 360; // 오차가 180도보다 크면 보정
            }

            // 디버깅 정보 출력
            Debug.Log($"Current Yaw: {yaw}, Goal Angle: {goalAngle}, Error: {error}");

            // 오차가 1도 미만이면 루프 탈출
            if (Mathf.Abs(error) < 1.0f)
            {
                break;
            }

            float pidOut = PIDController(error); // PID 제어기 출력 계산
            pidOut = Mathf.Clamp(pidOut, minOutput, maxOutput); // PID 출력값 제한

            // 로봇에 명령 전송
            Debug.Log($"Sending Twist Message: LinearX = {pidOut * orient}, LinearY = {-pidOut * orient}");
            SendTwistMessage(pidOut * orient, -pidOut * orient);

            transform.Rotate(Vector3.up, pidOut * orient * Time.deltaTime * rotationSpeed); // 로봇 회전

            yield return null; // 다음 프레임까지 대기
        }

        // 회전 정지
        Debug.Log("Stop Turning");
        SendTwistMessage(0, 0); // 정지 명령 전송
        isTurning = false; // 회전 상태 해제
    }

    private void SendTwistMessage(float linearX, float linearY)
    {
        // Twist 메시지 생성 및 설정
        TwistMsg twist = new TwistMsg
        {
            linear = new Vector3Msg
            {
                x = linearX,
                y = linearY,
                z = 0
            },
            angular = new Vector3Msg
            {
                x = 0,
                y = 0,
                z = 0
            }
        };
        ros.Publish(rosTopic, twist); // ROS 토픽으로 메시지 퍼블리시
    }

    private float PIDController(float error)
    {
        float currentTime = Time.time; // 현재 시간 가져오기
        float deltaTime = currentTime - previousTime; // 시간 변화량 계산

        integral += error * deltaTime; // 적분 값 업데이트
        float derivative = (error - previousError) / deltaTime; // 미분 값 계산
        float output = Kp * error + Ki * integral + Kd * derivative; // PID 출력 계산

        previousError = error; // 이전 오차 값 업데이트
        previousTime = currentTime; // 이전 시간 업데이트

        return output; // PID 출력 반환
    }
}
