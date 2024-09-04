using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    [RequireComponent(typeof(HitBox))]
    public class OnOverlapAttack : AttackBase
    {
        [Tooltip("Delay applied recursively to wait before applying the attack impact. If Duration is 0 this is only waited once")]
        public float DamageRate = 1f;
        [Tooltip("Duration passed before the attack deactivates. Set to -1 for infinite duration")]
        public float Duration = 0f;

        public UnityEvent OnAttackStart;
        public UnityEvent OnAttackEnd;

        private HitBox m_HitBox;
        private float m_CurrentDuration;
        private float m_Time;
        private List<Damageable> m_Damageables = new List<Damageable>();

        protected override void Awake()
        {
            base.Awake();

            m_HitBox = GetComponent<HitBox>();
        }

        private void OnEnable()
        {
            m_CurrentDuration = 0f;
            m_Time = 0;

            OnAttackStart?.Invoke();
            Hit();
        }

        private void OnDisable()
        {
            OnAttackEnd?.Invoke();
        }

        public void Update()
        {
            m_CurrentDuration += Time.deltaTime;
            m_Time += Time.deltaTime;
            if (m_Time >= DamageRate)
            {
                Hit();
            }

            if (Duration > 0 && m_CurrentDuration >= Duration)
            {
                enabled = false;
                return;
            }
        }

        private void Hit()
        {
            m_HitBox.GetOverlappingDamageables(m_Damageables);
            foreach (Damageable dmg in m_Damageables)
            {
                // TODO - Calculate real hitpoint, for now just an estimation
                Vector3 fakeHitPoint = (dmg.transform.position + m_HitBox.transform.position) * 0.5f;
                Vector3 impactDir = (dmg.transform.position - m_HitBox.transform.position).normalized;
                Process(new AttackInfo()
                {
                    Attack = this,
                    Damageable = dmg,
                    ImpactDir = impactDir,
                    ImpactPoint = fakeHitPoint
                });
            }
            m_Time = 0f;

            if (Duration == 0)
            {
                enabled = false;
            }
        }

        public override void StartAttack()
        {
            enabled = true;
        }

    }
}