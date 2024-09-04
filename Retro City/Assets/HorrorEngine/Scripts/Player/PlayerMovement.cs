using System;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorEngine
{
    public interface IPlayerMovementSettings
    {
        public PlayerMovementType GetMovementType();
        public float GetFwdRate(PlayerMovement movement);
        public float GetRightRate(PlayerMovement movement);
        public void GetRotation(PlayerMovement movement, out float sign, out float rate);
    }

    public class PlayerMovement : MonoBehaviour, IDeactivateWithActor
    {
        [Flags]
        public enum MovementConstrain
        {
            None            = 0,
            Movement        = 1,
            Rotation        = 2,
            MovementToAxis  = 4
        }



        [HideInInspector]
        public MovementConstrain Constrain = 0;

        [SerializeField] SettingsElementContent m_MovementTypeSetting;

        [Header("Main settings")]
        [SerializeField] float m_MovementSpeed;
        [SerializeField] float m_MovementRunSpeed;
        [SerializeField] float m_MovementBackwardsSpeed;
        [SerializeField] float m_MovementLateralSpeed;
        [SerializeField] float m_NavMeshCheckDistance;
        [SerializeField] bool m_AnalogRunning;
        [SerializeField] Vector3 m_Gravity = new Vector3(0, -9.8f, 0);

        [Header("Health Modifiers")]
        [SerializeField] bool m_ChangeWalkSpeedBasedOnHealth;
        [SerializeField] AnimationCurve m_NormalizedHealthSpeedScalar = AnimationCurve.Linear(0, 1f, 1f, 1f);
        [SerializeField] bool m_ChangeRunSpeedBasedOnHealth;
        [SerializeField] AnimationCurve m_NormalizedHealthRunSpeedScalar = AnimationCurve.Linear(0, 1f, 1f, 1f);

        private Vector2 m_InputAxis;
        private IPlayerInput m_Input;
        private bool m_Running;
        private Health m_Health;
        private IPlayerMovementSettings m_Settings;
        private CharacterController m_CharacterCtrl;

        public Vector3 LockedAxis { get; set; }
        public Vector3 IntendedMovement { get; private set; }
        public Vector2 InputAxis => m_InputAxis;


        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Input = GetComponent<IPlayerInput>();
            m_Health = GetComponent<Health>();
            m_CharacterCtrl = GetComponent<CharacterController>();
            
            UpdateMovementSettings();
            MessageBuffer<SettingsSavedMessage>.Subscribe(OnSettingsSavedMessage);
        }

        // --------------------------------------------------------------------

        private void OnDestroy()
        {
            MessageBuffer<SettingsSavedMessage>.Unsubscribe(OnSettingsSavedMessage);
        }

        // --------------------------------------------------------------------

        private void OnSettingsSavedMessage(SettingsSavedMessage msg)
        {
            UpdateMovementSettings();
        }

        // --------------------------------------------------------------------

        private void UpdateMovementSettings()
        {
            var settings = GetComponentsInChildren<IPlayerMovementSettings>();
            Debug.Assert(settings.Length > 0, "Character doesn't have any movement settings component");
            if (m_MovementTypeSetting)
            {
                PlayerMovementType movementType = m_MovementTypeSetting.GetAsEnum<PlayerMovementType>();
                foreach (var moveSettings in settings)
                {
                    if (moveSettings.GetMovementType() == movementType)
                    {
                        m_Settings = moveSettings;
                        break;
                    }
                }

                Debug.Assert(m_Settings != null, $"Movement settings of type {movementType} couldn't be found on the player");
            }

            if (m_Settings == null && settings.Length > 0)
            {
                m_Settings = settings[0];
            }
        }

        // --------------------------------------------------------------------

        private void Update()
        {
            if (!m_AnalogRunning)
                m_Running = m_Input.IsRunHeld();

            m_InputAxis = m_Input.GetPrimaryAxis();

        }

        // --------------------------------------------------------------------

        private void FixedUpdate()
        {
            if (!Constrain.HasFlag(MovementConstrain.Movement))
                UpdateMovement();

            if (!Constrain.HasFlag(MovementConstrain.Rotation))
                UpdateRotation();
        }

        // --------------------------------------------------------------------

        private void UpdateRotation()
        {
            m_Settings.GetRotation(this, out float sign, out float rate);
            if (rate > 0)
                Rotate(sign, rate);
        }

        // --------------------------------------------------------------------

        public void Rotate(float dir, float speed)
        {
            transform.rotation = transform.rotation * Quaternion.Euler(Vector3.up * dir * Time.deltaTime * speed);
        }

        // --------------------------------------------------------------------

        private void UpdateMovement()
        {
            Vector3 movement = Vector3.zero;
            movement += GetForwardMovement(out float absFwd);
            movement += GetRightMovement();

            IntendedMovement = movement;

            if (Constrain.HasFlag(MovementConstrain.MovementToAxis))
            {
                IntendedMovement = Vector3.Project(IntendedMovement, LockedAxis);
            }

            Vector3 prevPos = transform.position;
            Vector3 newPos = prevPos + IntendedMovement;

            Debug.DrawLine(prevPos, prevPos + IntendedMovement * 10, Color.red);

            Vector3 finalMove = newPos - prevPos;
            m_CharacterCtrl.Move(finalMove + m_Gravity * Time.deltaTime);
        }

        // --------------------------------------------------------------------

        Vector3 GetForwardMovement(out float absFwd)
        {
            float fwd = m_Settings.GetFwdRate(this);
            absFwd = Mathf.Abs(fwd);

            float speed = 0f;
            if (m_AnalogRunning)
            {
                if (fwd > Mathf.Epsilon)
                    speed = Mathf.Lerp(m_MovementSpeed * absFwd, m_MovementRunSpeed, absFwd);
                else if (fwd < -Mathf.Epsilon)
                    speed = m_MovementBackwardsSpeed * absFwd;
            }
            else
            {
                if (fwd > Mathf.Epsilon)
                    speed = m_Running ? m_MovementRunSpeed : m_MovementSpeed;
                else if (fwd < -Mathf.Epsilon)
                    speed = m_MovementBackwardsSpeed;
            }

            if (speed >= m_MovementRunSpeed && m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthRunSpeedScalar.Evaluate(m_Health.Normalized);
            else if (m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthSpeedScalar.Evaluate(m_Health.Normalized);

            return transform.forward * Time.deltaTime * speed * Mathf.Sign(fwd);
        }

        // --------------------------------------------------------------------

        Vector3 GetRightMovement()
        {
            float right = m_Settings.GetRightRate(this);
            float absRight = Mathf.Abs(right);

            float speed = 0f;
            if (m_AnalogRunning)
            {
                if (right > Mathf.Epsilon || right < -Mathf.Epsilon)
                    speed = m_MovementLateralSpeed * absRight;
            }
            else
            {
                if (right > Mathf.Epsilon || right < -Mathf.Epsilon)
                    speed = m_MovementLateralSpeed;
            }

            if (speed >= m_MovementRunSpeed && m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthRunSpeedScalar.Evaluate(m_Health.Normalized);
            else if (m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthSpeedScalar.Evaluate(m_Health.Normalized);

            return transform.right * Time.deltaTime * speed * Mathf.Sign(right);
        }

        // --------------------------------------------------------------------

        public void AddConstrain(MovementConstrain constrain) { Constrain |= constrain; }
        public void RemoveConstrain(MovementConstrain constrain) { Constrain &= ~constrain; }


        // --------------------------------------------------------------------

        void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position - transform.right, transform.position + transform.right);
            Gizmos.DrawLine(transform.position - transform.forward, transform.position + transform.forward);
        }

    }
}