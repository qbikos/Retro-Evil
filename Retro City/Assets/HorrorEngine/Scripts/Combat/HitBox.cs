using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class HitBox : MonoBehaviour
    {
        [SerializeField] private Vector3 m_Center;
        [SerializeField] private Vector3 m_Size = Vector3.one;
        [SerializeField] private LayerMask m_LayerMask;

        private Collider[] m_OverlapResults = new Collider[10];

        // --------------------------------------------------------------------

        public void GetOverlappingDamageables(List<Damageable> damageables)
        {
            damageables.Clear();
            
            var scaledCenter = new Vector3(m_Center.x * transform.lossyScale.x, m_Center.y * transform.lossyScale.y, m_Center.z * transform.lossyScale.z);
            var scaledSize = new Vector3(m_Size.x * transform.lossyScale.x, m_Size.y * transform.lossyScale.y, m_Size.z * transform.lossyScale.z);

            DebugUtils.DrawBox(transform.position + scaledCenter, transform.rotation, scaledSize, Color.red, 10);
            int count = Physics.OverlapBoxNonAlloc(transform.position + scaledCenter, scaledSize * 0.5f, m_OverlapResults, transform.rotation, m_LayerMask, QueryTriggerInteraction.Collide);
            
            for (int i = 0; i < count; ++i)
            {
                if (m_OverlapResults[i].TryGetComponent(out Damageable d))
                    damageables.Add(d);
            }
        }

        // --------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.red;

                var scaledCenter = new Vector3(m_Center.x * transform.lossyScale.x, m_Center.y * transform.lossyScale.y, m_Center.z * transform.lossyScale.z);
                var scaledSize = new Vector3(m_Size.x * transform.lossyScale.x, m_Size.y * transform.lossyScale.y, m_Size.z * transform.lossyScale.z);

                Gizmos.matrix = Matrix4x4.Rotate(transform.rotation);
                Gizmos.DrawWireCube(transform.position + scaledCenter, scaledSize);
                Gizmos.matrix = Matrix4x4.identity;

                Gizmos.color = Color.white;
            }
        }
    }
}