using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class EnemyStateKnockback : ActorState
    {
        [SerializeField] private float m_Force;
        [SerializeField] private float m_Drag;
        [SerializeField] private ActorState m_ExitState;

        private Rigidbody m_Rigidbody;
        private Vector3 m_Velocity;

        protected override void Awake()
        {
            base.Awake();

            m_Rigidbody = GetComponentInParent<Rigidbody>();
        }

        public override void StateEnter(IActorState fromState)
        {
            base.StateEnter(fromState);

            m_Rigidbody.isKinematic = false;
            m_Velocity = -Actor.transform.forward * m_Force;
        }

        public override void StateExit(IActorState intoState)
        {
            m_Rigidbody.isKinematic = true;

            base.StateExit(intoState);
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();

            m_Velocity = m_Velocity * (1 - m_Drag);
            m_Rigidbody.velocity = m_Velocity;

            if (m_Rigidbody.velocity.magnitude < 0.1f)
                SetState(m_ExitState);
        }
    }
}