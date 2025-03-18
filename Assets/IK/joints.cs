using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Robotics
{
    public class customJoint : MonoBehaviour
    {
        public customJoint m_child;

        public customJoint GetChild()
        {
            return m_child;
        }

        public void Rotate(float _angle)
        {
            // 회전 축을 up으로 설정
            transform.Rotate(Vector3.up * _angle);
        }
}
}
