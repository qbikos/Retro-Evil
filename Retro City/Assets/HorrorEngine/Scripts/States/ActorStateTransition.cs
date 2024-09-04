using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class ActorStateTransition : MonoBehaviour
    {
        [SerializeField] private bool m_FromAllStates;
        [SerializeField] private List<ActorState> m_FromStates;
        [SerializeField] private List<ActorState> m_ExcludeStates;
        [SerializeField] private ActorState m_ToState;

        private ActorStateController m_StateController;

        private void Awake()
        {
            m_StateController = GetComponentInParent<ActorStateController>();
        }

        public void Trigger()
        {
            if (m_FromAllStates || m_FromStates.Contains(m_StateController.CurrentState as ActorState))
            {
                if (!m_ExcludeStates.Contains(m_StateController.CurrentState as ActorState))
                    m_StateController.SetState(m_ToState);
            }
        }
    }
}