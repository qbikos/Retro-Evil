using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public class DepletableLight : MonoBehaviour
    {
        [FormerlySerializedAs("m_IntensityOverStatus")]
        [SerializeField] public AnimationCurve m_MultiplierOverStatus;
        [SerializeField] public float m_InterpolationSpeed = 3f;

        private Light m_Light;
        private LensFlare m_Flare;

        private float m_OriginalIntensity;
        private float m_OriginalFlareBrightness;
        private float m_Status;

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Light = GetComponent<Light>();
            m_Flare = GetComponent<LensFlare>();
            if (m_Flare)
                m_OriginalFlareBrightness = m_Flare.brightness;

            m_OriginalIntensity = m_Light.intensity;
            m_Status = 1f;
        }

        // --------------------------------------------------------------------

        public void OnDeplete(float status)
        {
            m_Status = status;
        }

        // --------------------------------------------------------------------

        private void Update()
        {
            float targetIntensity = m_OriginalIntensity * m_MultiplierOverStatus.Evaluate(m_Status);
            m_Light.intensity = Mathf.MoveTowards(m_Light.intensity, targetIntensity, Time.deltaTime * m_InterpolationSpeed);

            if (m_Flare)
            {
                float targetBrightness = m_OriginalFlareBrightness * m_MultiplierOverStatus.Evaluate(m_Status);
                m_Flare.brightness = Mathf.MoveTowards(m_Flare.brightness, targetBrightness, Time.deltaTime * m_InterpolationSpeed);
            }
        }

    }
}