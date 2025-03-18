using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    public float speed = 5f;  // 오브젝트의 이동 속도
    public float maxDistance = 1f;  // 오브젝트가 이동할 수 있는 최대 거리

    private Vector3 startPosition;

    void Start()
    {
        // 오브젝트의 초기 위치 저장
        startPosition = transform.position;
    }

    void Update()
    {
        // 현재의 위치
        Vector3 currentPosition = transform.position;

        // 입력 값에 따라 이동할 거리 계산 (방향을 반대로)
        float moveInput = -Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        // 새로운 위치 계산, X와 Y는 현재 위치를 유지하고 Z만 변경
        Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + moveInput);

        // 새로운 위치가 초기 위치로부터 maxDistance 이내인지 확인
        if (Mathf.Abs(newPosition.z - startPosition.z) <= maxDistance)
        {
            // 이동이 최대 거리를 벗어나지 않으면 이동 적용
            transform.position = newPosition;
        }
        else
        {
            // 이동이 최대 거리를 벗어날 경우 최대 거리에서 멈추게 함
            if (moveInput > 0)  // 오른쪽(앞쪽) 이동 시
            {
                transform.position = new Vector3(currentPosition.x, currentPosition.y, startPosition.z + maxDistance);
            }
            else if (moveInput < 0)  // 왼쪽(뒤쪽) 이동 시
            {
                transform.position = new Vector3(currentPosition.x, currentPosition.y, startPosition.z - maxDistance);
            }
        }
    }
}
