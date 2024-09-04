using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public class ActorStateController : MonoBehaviour, IResetable
    {
        [SerializeField] ActorState m_InitialState;
        [SerializeField] bool m_ShowDebug;

        public IActorState CurrentState { get; private set; }

        private IActorState m_NewState;
        private Coroutine m_ExitStateRoutine;
        private ActorState[] m_States;
        private Dictionary<string, List<ActorState>> m_HashedTagStates = new Dictionary<string, List<ActorState>>();

        public UnityEvent<IActorState, IActorState> OnStateChanged;

        // --------------------------------------------------------------------

        private void Start()
        {
            m_States = GetComponentsInChildren<ActorState>();

            foreach(var state in m_States)
            {
                foreach (var tag in state.Tags)
                {
                    if (!m_HashedTagStates.ContainsKey(tag))
                    {
                        m_HashedTagStates.Add(tag, new List<ActorState>());
                    }

                    m_HashedTagStates[tag].Add(state);
                }
            }
        }

        // --------------------------------------------------------------------

        private void OnEnable()
        {
            SetState(m_InitialState);
        }

        // --------------------------------------------------------------------

        public void OnReset()
        {
            SetState(m_InitialState, true);
        }

        // --------------------------------------------------------------------

        public void SetState(IActorState state, bool immediate = false)
        {
            m_NewState = state;
            if (immediate)
                SetState_Internal(m_NewState);
        }

        // --------------------------------------------------------------------

        void OnGUI()
        {
            if (m_ShowDebug)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 24;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.yellow;

                GUILayout.BeginArea(new Rect(10, 10, Screen.width, Screen.height));
                GUILayout.Label(name + ":" + (CurrentState != null ? CurrentState.GetType().Name : "NONE"), style);
                GUILayout.EndArea();
            }
        }

        // --------------------------------------------------------------------

        private void Update()
        {
            if (PauseController.Instance.IsPaused)
                return;

            if (m_ExitStateRoutine != null)
                return;

            if (m_NewState != null)
            {
                SetState_Internal(m_NewState);
            }
            else
            {
                // This is important, we want to leave one frame between the state change so the input refreshes
                CurrentState.StateUpdate();
            }
        }

        // --------------------------------------------------------------------

        private void SetState_Internal(IActorState state)
        {
            IActorState prevState = CurrentState;
            if (CurrentState != null)
                CurrentState.StateExit(state);

            ActorState prevAState = prevState as ActorState;
            ActorState newAState = state as ActorState;
            if (!newAState.SkipPreviousExitAnimation && prevAState && prevAState.HasExitAnimation())
            {
                m_ExitStateRoutine = StartCoroutine(EnterStateDelayed(prevAState.ExitDuration, prevState, state));
            }
            else
            {
                EnterState(prevState, state);
            }
        }

        // --------------------------------------------------------------------

        private IEnumerator EnterStateDelayed(float delay, IActorState prevState, IActorState nextState)
        {
            yield return Yielders.Time(delay);
            EnterState(prevState, nextState);
        }

        // --------------------------------------------------------------------


        private void EnterState(IActorState fromState, IActorState toState)
        {
            if (m_ExitStateRoutine != null)
            {
                StopCoroutine(m_ExitStateRoutine);
                m_ExitStateRoutine = null;
            }

            CurrentState = toState;
            CurrentState.StateEnter(fromState);

            if (m_NewState == toState) // NewState could have changed in StateEnter
                m_NewState = null;

            OnStateChanged?.Invoke(fromState, toState);
        }

        // --------------------------------------------------------------------

        private void FixedUpdate()
        {
            CurrentState?.StateFixedUpdate();
        }

        // --------------------------------------------------------------------

        public ActorState GetWithTag(string tag)
        {
            Debug.Assert(m_HashedTagStates.ContainsKey(tag), $"State with tag {tag} not found on actor", gameObject);
            return m_HashedTagStates[tag][0];
        }

        // --------------------------------------------------------------------

        private void OnDisable()
        {
            if (CurrentState != null)
                CurrentState.StateExit(null);

            CurrentState = null;
        }
    }
}