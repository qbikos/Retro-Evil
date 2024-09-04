using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{

    public class ActorStateWithDuration : ActorState
    {
        [SerializeField] protected float m_Duration;
        [SerializeField] protected ActorState m_GoToStateAfterDuration;

        protected float m_TimeInState;
        private bool m_Finished;

        // --------------------------------------------------------------------

        public override void StateEnter(IActorState fromState)
        {
            base.StateEnter(fromState);

            m_Finished = false;
            m_TimeInState = 0f;
        }

        // --------------------------------------------------------------------

        public override void StateUpdate()
        {
            base.StateUpdate();

            m_TimeInState += Time.deltaTime;
            if (m_TimeInState > m_Duration && !m_Finished)
            {
                OnStateDurationEnd();
                m_Finished = true;
            }            
        }

        // --------------------------------------------------------------------

        protected virtual void OnStateDurationEnd()
        {
            if (m_GoToStateAfterDuration)
            {
                SetState(m_GoToStateAfterDuration);
            }
        }
    }
}