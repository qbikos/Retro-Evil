using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class PlayerVerticalAimingFromTransform : MonoBehaviour, IPlayerVerticalAiming
    {
        [SerializeField] Transform m_Transform;
        [SerializeField] float m_MinY = -1;
        [SerializeField] float m_MaxY = 1;
        [SerializeField] float m_Smoothness = 1;
        private float m_Verticality;

        public float Verticality { get => m_Verticality; set { } }

        private void OnEnable()
        {
            enabled = m_Transform != null;
        }

        public void SetTransform(Transform t)
        {
            m_Transform = t;
            enabled = true;
        }

        private void Update()
        {
            float desiredVerticality = MathUtils.Map(m_Transform.forward.y, m_MinY, m_MaxY, -1, 1);
            m_Verticality = Mathf.Lerp(m_Verticality, desiredVerticality, Time.deltaTime * m_Smoothness);
        }
    }
}