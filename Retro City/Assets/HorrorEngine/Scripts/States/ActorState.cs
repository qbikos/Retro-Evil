using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public interface IActorState
    {
        void StateEnter(IActorState fromState);

        void StateExit(IActorState intoState);

        void StateUpdate();

        void StateFixedUpdate();

        bool HasTag(string tag);
    }

    [System.Serializable]
    public class StateTransition
    {
        public ActorState FromState;
        public AnimatorStateHandle AnimationState;
        public float AnimationBlendTime = 0.25f;
    }

    public class ActorState : MonoBehaviour, IResetable, IActorState
    {
        [SerializeField] protected AnimatorStateHandle m_AnimationState;
        [SerializeField] protected float m_AnimationBlendTime = 0.25f;

        [SerializeField] protected StateTransition[] m_StateTransitions;

        [SerializeField] protected AnimatorStateHandle m_ExitAnimationState;
        [SerializeField] protected float m_ExitAnimationBlendTime = 0.25f;
        [SerializeField] protected float m_ExitAnimationDuration = 0f;

        [Tooltip("If this is set to true the ExitAnimation of the previous state will be ignored when entering this state")]
        public bool SkipPreviousExitAnimation;

        public UnityEvent OnStateEnter;
        public UnityEvent OnStateExit;

        public string[] Tags;

        private HashSet<string> m_HashedTags = new HashSet<string>();

        protected Actor Actor { get; private set; }
        protected bool TransitionFinished => m_TransitionTime <= 0;
        private float m_TransitionTime;

        private Coroutine m_EnterRoutine;


        // --------------------------------------------------------------------

        public bool HasExitAnimation() { return m_ExitAnimationState && m_ExitAnimationDuration > 0f; }
        public float ExitDuration => m_ExitAnimationDuration;

        // --------------------------------------------------------------------

        protected virtual void Awake()
        {
            foreach (var tag in Tags)
                m_HashedTags.Add(tag);

            Actor = GetComponentInParent<Actor>();
        }

        // --------------------------------------------------------------------

        public virtual void StateEnter(IActorState fromState)
        {
            bool foundStateTransition = false;
            if (m_StateTransitions != null && m_StateTransitions.Length > 0)
            {
                foreach (var specific in m_StateTransitions)
                {
                    if (specific.FromState == (ActorState)fromState)
                    {
                        Actor.MainAnimator.CrossFadeInFixedTime(specific.AnimationState ? specific.AnimationState.Hash : m_AnimationState.Hash, specific.AnimationBlendTime);
                        foundStateTransition = true;
                        m_TransitionTime = specific.AnimationBlendTime;
                        break;
                    }
                }
            }

            if (!foundStateTransition)
            {
                if (m_AnimationState)
                {
                    Actor.MainAnimator.CrossFadeInFixedTime(m_AnimationState.Hash, m_AnimationBlendTime);
                    m_TransitionTime = m_AnimationBlendTime;
                }
            }

            OnStateEnter?.Invoke();
        }

        // --------------------------------------------------------------------

        public virtual void StateUpdate()
        {
            if (m_TransitionTime > 0)
            {
                m_TransitionTime -= Time.deltaTime;
                if (TransitionFinished)
                    OnTransitionFinished();
            }
        }

        // --------------------------------------------------------------------

        public virtual void StateFixedUpdate()
        {
        }

        // --------------------------------------------------------------------

        public virtual void StateExit(IActorState intoState)
        {
            if (m_EnterRoutine != null)
            {
                StopCoroutine(m_EnterRoutine);
                m_EnterRoutine = null;
            }

            if (m_ExitAnimationState && m_ExitAnimationDuration > 0f)
            {
                Actor.MainAnimator.CrossFadeInFixedTime(m_ExitAnimationState.Hash, m_ExitAnimationBlendTime);
            }

            OnStateExit?.Invoke();
        }

        // --------------------------------------------------------------------

        protected void SetState(IActorState state, bool immediate = false)
        {
            Debug.Assert(state != null, "State can't be null");
            Actor.StateController.SetState(state, immediate);
        }

        // --------------------------------------------------------------------

        public virtual void OnReset()
        {
        }

        // --------------------------------------------------------------------

        public bool HasTag(string tag) { return m_HashedTags.Contains(tag); }

        // --------------------------------------------------------------------

        protected virtual void OnTransitionFinished()
        {
            // Override in child states
        }
    }
}