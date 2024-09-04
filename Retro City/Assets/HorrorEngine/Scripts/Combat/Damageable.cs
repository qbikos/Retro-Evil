using System;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
   
    [Serializable]
    public class OnDamageEvent : UnityEvent<Vector3, Vector3> { }

    public class Damageable : MonoBehaviour
    {
        public Combatant Owner { get; private set; }

        public DamageableType Type;

        [Tooltip("Event fired before the damage is applied")]
        public OnDamageEvent OnPreDamage;
        [Tooltip("Event fired after the damage has been applied")]
        public OnDamageEvent OnDamage;


        private Health m_Health;

        private void Awake()
        {
            Owner = GetComponentInParent<Combatant>();
            m_Health = GetComponentInParent<Health>();
        }

        public void Damage(float damage, Vector3 impactPoint, Vector3 impactDir)
        {
            if (!m_Health.IsDead)
            {
                if (damage > 0)
                {
                    OnPreDamage?.Invoke(impactPoint, impactDir);

                    m_Health.TakeDamage(damage, this);

                    OnDamage?.Invoke(impactPoint, impactDir);
                }

            }
        }
    }
}