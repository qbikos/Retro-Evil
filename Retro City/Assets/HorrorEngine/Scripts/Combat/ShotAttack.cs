using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
   

    interface IShotAttackAiming
    {
        Vector3 GetDirection();
    }

    public struct ShotHit
    {
        public Damageable Damageable;
        public Vector3 ImpactPoint;
        public Vector3 ImpactNormal;
    }

    [RequireComponent(typeof(SocketController))]
    [RequireComponent(typeof(AudioSource))]
    public class ShotAttack : WeaponAttack, IVerticalAttack
    {
        [SerializeField] private SocketHandle m_AttackOriginSocket;
        [SerializeField] private float m_NoAmmoAttackDuration;

        [Header("Attack Visuals")]
        [SerializeField] private ObjectInstantiator m_MuzzleInstantiator;
        [SerializeField] private ObjectInstantiator m_ShellKickInstantiator;
        [SerializeField] private float m_ShellKickDelay;

        [Header("Attack Size & Range")]
        [SerializeField] private float m_CastRadius = 0.5f;
        [SerializeField] private float m_MaxRange = 100f;
        [SerializeField] private float m_ForwardOffset = 0f;
        [Range(1, 10)]
        [SerializeField] private int m_PenetrationHits = 1;
        [SerializeField] private LayerMask m_LayerMask;
        [SerializeField] private LayerMask m_ObstructionLayerMask;
        [SerializeField] private float m_VerticalityFactor;

        [Space]
        [SerializeField] private bool m_ShowDebug;

        private AudioSource m_AudioSource;
        private RaycastHit[] m_HitResults = new RaycastHit[10];
        
        private List<ShotHit> m_SortedHits = new List<ShotHit>();
        private SocketController m_SocketCtrl;
        private float m_Verticality;
        
        private IShotAttackAiming m_AttackAiming;

        // --------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            m_SocketCtrl = GetComponent<SocketController>();
            m_AudioSource = GetComponent<AudioSource>();
        }

        // --------------------------------------------------------------------

        private void OnEnable()
        {
            m_AttackAiming = GetComponentInParent<IShotAttackAiming>();
        }

        // --------------------------------------------------------------------

        public void SetVerticality(float verticality)
        {
            m_Verticality = verticality;
        }

        // --------------------------------------------------------------------

        public override void StartAttack()
        {
            Socket originSocket = m_SocketCtrl.GetSocket(m_AttackOriginSocket);
            Vector3 originPos = originSocket.transform.position + originSocket.transform.forward * m_ForwardOffset;


            Vector3 dir = originSocket.transform.forward;

            if (m_AttackAiming == null)
            {
                dir = Vector3.Slerp(new Vector3(dir.x, 0, dir.z).normalized, dir.normalized, Mathf.Abs(m_Verticality) * m_VerticalityFactor);
                dir.Normalize();
            }
            else
            {
                dir = m_AttackAiming.GetDirection();
            }

            if (m_MuzzleInstantiator)
                m_MuzzleInstantiator.Instatiate();

            if (m_ShellKickInstantiator)
                Invoke(nameof(KickShell), m_ShellKickDelay);

            float maxRange = GetNonObstructedMaxRange(originPos, dir);

#if UNITY_EDITOR
            if (m_ShowDebug)
            {
                Debug.DrawLine(originPos, originPos + dir * maxRange, Color.red, 10f);
                Debug.DrawLine(originPos + Vector3.up * m_CastRadius, originPos + Vector3.up * m_CastRadius + dir * maxRange, Color.red, 10f);
                Debug.DrawLine(originPos + Vector3.down * m_CastRadius, originPos + Vector3.down * m_CastRadius + dir * maxRange, Color.red, 10f);
                Debug.DrawLine(originPos + Vector3.left * m_CastRadius, originPos + Vector3.left * m_CastRadius + dir * maxRange, Color.red, 10f);
                Debug.DrawLine(originPos + Vector3.right * m_CastRadius, originPos + Vector3.right * m_CastRadius + dir * maxRange, Color.red, 10f);
            }
#endif

            int hits = Physics.SphereCastNonAlloc(new Ray(originPos, dir), m_CastRadius, m_HitResults, maxRange, m_LayerMask, QueryTriggerInteraction.Collide);
            if (hits > 0)
            {
                GetAndSortDamageables(hits);

                int hitCount = 0;
                foreach (var hit in m_SortedHits)
                {
                    if (hit.Damageable)
                    {
                        Process(new AttackInfo()
                        {
                            Attack = this,
                            Damageable = hit.Damageable,
                            ImpactDir = -hit.ImpactNormal,
                            ImpactPoint = hit.ImpactPoint
                        });
                    }

                    ++hitCount;
                    if (hitCount >= m_PenetrationHits)
                        break;
                }

                
            }
            
            ReloadableWeaponData reloadable = m_WeaponData as ReloadableWeaponData;
            if (reloadable.ShotSound)
                m_AudioSource.PlayOneShot(reloadable.ShotSound);
        }

        // --------------------------------------------------------------------

        private float GetNonObstructedMaxRange(Vector3 originPos, Vector3 dir)
        {
            float maxRange = 0;
            RaycastHit hit;

            Physics.Raycast(originPos, dir, out hit, m_MaxRange, m_ObstructionLayerMask);
            maxRange = Mathf.Max(maxRange, hit.distance);
            Physics.Raycast(originPos + Vector3.up * m_CastRadius,  dir, out hit, m_MaxRange, m_ObstructionLayerMask);
            maxRange = Mathf.Max(maxRange, hit.distance);
            Physics.Raycast(originPos + Vector3.down * m_CastRadius, dir, out hit, m_MaxRange, m_ObstructionLayerMask);
            maxRange = Mathf.Max(maxRange, hit.distance);
            Physics.Raycast(originPos + Vector3.left * m_CastRadius, dir, out hit, m_MaxRange, m_ObstructionLayerMask);
            maxRange = Mathf.Max(maxRange, hit.distance);
            Physics.Raycast(originPos + Vector3.right * m_CastRadius, dir, out hit, m_MaxRange, m_ObstructionLayerMask);
            maxRange = Mathf.Max(maxRange, hit.distance);

            maxRange = Mathf.Min(maxRange, m_MaxRange);

            return maxRange;
        }

        // --------------------------------------------------------------------

        public override void OnAttackNotStarted()
        {
            ReloadableWeaponData reloadable = m_WeaponData as ReloadableWeaponData;
            if (reloadable.NoAmmoSound)
                m_AudioSource.PlayOneShot(reloadable.NoAmmoSound);
        }

        // --------------------------------------------------------------------

        private void KickShell()
        {
            m_ShellKickInstantiator.Instatiate();
        }

        // --------------------------------------------------------------------

        private void GetAndSortDamageables(int count)
        {
            m_SortedHits.Clear();
            float minDist = float.MaxValue;
            float maxDist = 0;
            for(int i = 0; i < count; ++i)
            {
                Damageable damageable = m_HitResults[i].collider.GetComponent<Damageable>();
                AttackImpact impact = damageable ? m_Attack.GetImpact(damageable.Type) : null;
                if (impact != null && impact.Damage > 0.0f)
                {
                    float dist = m_HitResults[i].distance;
                    ShotHit hit = new ShotHit()
                    {
                        Damageable = damageable,
                        ImpactPoint = m_HitResults[i].point,
                        ImpactNormal = m_HitResults[i].normal
                    };

                    if (m_SortedHits.Count == 0)
                    {
                        m_SortedHits.Add(hit);
                        minDist = dist;
                        maxDist = dist;
                    }                        
                    else if (dist <= minDist)
                    {
                        m_SortedHits.Insert(0, hit);
                        minDist = dist;
                    }
                    else if (dist >= maxDist)
                    {
                        m_SortedHits.Insert(m_SortedHits.Count, hit);
                        maxDist = dist;
                    }
                }
            }
        }

        // --------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            m_SocketCtrl = GetComponent<SocketController>();
            if (m_SocketCtrl)
            {
                Gizmos.color = Color.red;
                Socket originSocket = m_SocketCtrl.GetSocket(m_AttackOriginSocket);
                Vector3 sphereCastMidpoint = originSocket.transform.position + (originSocket.transform.forward * (m_MaxRange - m_CastRadius));
                Gizmos.DrawWireSphere(sphereCastMidpoint, m_CastRadius);
                Gizmos.DrawWireSphere(originSocket.transform.position + originSocket.transform.forward * m_ForwardOffset, m_CastRadius);
                Debug.DrawLine(originSocket.transform.position, sphereCastMidpoint, Color.red);
            }
        }
#endif

    }
}