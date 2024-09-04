using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public class MeleeAttack : WeaponAttack, IVerticalAttack
    {
        [Header("Melee Specs")]
        [SerializeField] private HitBox m_Hitbox;
        [FormerlySerializedAs("m_Duration")]
        [SerializeField] private float m_HitDuration;
        
        private float m_Time;
        private List<Damageable> m_Damageables = new List<Damageable>();
        private HashSet<Damageable> m_AlreadyHitDamageables = new HashSet<Damageable>();
        private float m_Verticality;

        // --------------------------------------------------------------------

        private void Start()
        {
            enabled = false;
        }

        // --------------------------------------------------------------------

        public override void StartAttack()
        {
            m_Damageables.Clear();
            m_AlreadyHitDamageables.Clear();

            m_Time = 0f;
            enabled = true;
        }

        // --------------------------------------------------------------------

        public void Update()
        {
            m_Hitbox.GetOverlappingDamageables(m_Damageables);
            foreach (var damageable in m_Damageables)
            {
                if (!m_AlreadyHitDamageables.Contains(damageable))
                {
                    // TODO - Calculate real hitpoint, for now just an estimation
                    Vector3 fakeHitPoint = (damageable.transform.position + m_Hitbox.transform.position) * 0.5f;
                    Vector3 impactDir = (damageable.transform.position - m_Hitbox.transform.position).normalized;
                    Process(new AttackInfo()
                    {
                        Attack = this,
                        Damageable = damageable,
                        ImpactDir = impactDir,
                        ImpactPoint = fakeHitPoint
                    });

                    m_AlreadyHitDamageables.Add(damageable);
                }
            }

            m_Time += Time.deltaTime;
            if (m_Time > m_HitDuration)
            {
                enabled = false;
            }
        }
        
        // --------------------------------------------------------------------

        public void SetVerticality(float verticality)
        {
            m_Verticality = verticality;
        }
    }
}
