using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Robotics_1
{
    public class CustomJoint_1 : MonoBehaviour
    {
        public CustomJoint_1 m_child;
        public int jointNumber;  // 조인트의 번호

        public CustomJoint_1 GetChild()
        {
            return m_child;
        }

        public void Rotate(float angle)
        {
            transform.Rotate(Vector3.right * angle);
        }
        public void Rotate_top(float angle)
        {
            transform.Rotate(Vector3.forward * angle);
        }

        // 유니티의 매 프레임마다 호출되는 함수
        void Update()
        {   if (Input.GetKey(KeyCode.UpArrow))
            {
                if (jointNumber == 0) // 짝수 조인트는 반대 방향으로 회전
                {
                    Rotate(1.5f);
                }
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (jointNumber == 0) // 짝수 조인트는 반대 방향으로 회전
                {
                    Rotate(-1.5f);
                }
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (jointNumber == 12) // 짝수 조인트는 반대 방향으로 회전
                {
                    Rotate_top(-0.5f);
                }
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (jointNumber == 12) // 짝수 조인트는 반대 방향으로 회전
                {
                    Rotate_top(0.5f);
                }
            }
            // 위쪽 방향키가 눌렸을 때
            if (Input.GetKey(KeyCode.UpArrow) && jointNumber != 12)
            {
                if (jointNumber % 2 == 0 && jointNumber != 0) // 짝수 조인트는 반대 방향으로 회전
                {
                    Rotate(1f);
                }
                else // 홀수 조인트는 정방향으로 회전
                {
                    Rotate(-1f);
                }
            }

            // 아래쪽 방향키가 눌렸을 때
            if (Input.GetKey(KeyCode.DownArrow) && jointNumber != 12)
            {
                if (jointNumber % 2 == 0 && jointNumber != 0) // 짝수 조인트는 정방향으로 회전
                {
                    Rotate(-1f);
                }
                else // 홀수 조인트는 반대 방향으로 회전
                {
                    Rotate(1f);
                }
            }
        }
    }
}
