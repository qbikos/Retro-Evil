using UnityEngine;
using UnityEngine.UI;

namespace HorrorEngine 
{

    [System.Serializable]
    public class UIPlayerStatusEntry
    {
        public float FromHealth;
        public Color Color;
        public float Tiling = 1f;
        public float Interval = 1f;
        public float Speed = 1f;
        public string Text;
    }
    public class UIPlayerStatus : MonoBehaviour
    {
        [SerializeField] private UIPlayerStatusEntry[] Status;

        [SerializeField] private UIStatusLine m_Line;
        [SerializeField] private Image m_StatusBg;
        [SerializeField] private TMPro.TextMeshProUGUI m_StatusText;

        private Health m_Health;

        // --------------------------------------------------------------------

        private void Start()
        {
            if (GameManager.Exists)
            {
                if (GameManager.Instance.Player)
                {
                    BindToPlayer(GameManager.Instance.Player);
                }
                else
                {
                    GameManager.Instance.OnPlayerRegistered.AddListener(OnPlayerRegistered);
                }
            }
        }

        // --------------------------------------------------------------------

        private void OnEnable()
        {
            if (m_Health)
                UpdateStatus();
        }

        // --------------------------------------------------------------------

        private void OnPlayerRegistered(PlayerActor player)
        {
            BindToPlayer(player);
        }

        // --------------------------------------------------------------------

        private void BindToPlayer(PlayerActor player)
        {
            if (m_Health)
                m_Health.OnHealthAltered.RemoveListener(OnHealthAltered);

            m_Health = player.GetComponent<Health>();
            m_Health.OnHealthAltered.AddListener(OnHealthAltered);
            
            UpdateStatus();
        }

        // --------------------------------------------------------------------

        private void OnDestroy()
        {
            if (m_Health)
                m_Health.OnHealthAltered.RemoveListener(OnHealthAltered);

            GameManager.Instance.OnPlayerRegistered.RemoveListener(OnPlayerRegistered);
        }

        // --------------------------------------------------------------------

        private void OnHealthAltered(float prev, float current)
        {
            UpdateStatus();
        }

        // --------------------------------------------------------------------

        private void UpdateStatus()
        {
            
            float maxHealth = 0;
            UIPlayerStatusEntry selectedStatus = null;
            foreach (var state in Status)
            {
                if (selectedStatus == null || (m_Health.Value >= state.FromHealth && state.FromHealth > maxHealth))
                {
                    maxHealth = state.FromHealth;
                    selectedStatus = state;
                }
            }

            m_StatusBg.color = selectedStatus.Color;
            m_StatusText.text = selectedStatus.Text;
            m_Line.SetStatus(selectedStatus);
            
        }
    }
}
