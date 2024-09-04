using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public class SaveStation : MonoBehaviour
    {
        [SerializeField] private string m_LocationName;
        [Tooltip("If an item is not specified, players will be able to save as many times they want")]
        [SerializeField] private ItemData m_ItemRequiredToSave;

        [SerializeField] private DialogData m_OnUseDialog;
        [SerializeField] private DialogData m_OnNoSaveItemDialog;

        private Choice m_SaveChoice;

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_SaveChoice = GetComponent<Choice>();
        }

        // --------------------------------------------------------------------

        public void Use()
        {
            if (m_OnUseDialog.IsValid())
            {
                UIManager.PushAction(new UIStackedAction()
                {
                    Action = () =>
                    {
                        ShowChoice();
                    },
                    StopProcessingActions = true,
                    Name = "SaveStation.Use (ShowChoice)"
                });
                UIManager.Get<UIDialog>().Show(m_OnUseDialog);
            }
            else
            {
                ShowChoice();
            }
        }

        // --------------------------------------------------------------------

        private void ShowChoice()
        {
            if (m_ItemRequiredToSave)
            {
                if (!GameManager.Instance.Inventory.Contains(m_ItemRequiredToSave))
                {
                    UIManager.Get<UIDialog>().Show(m_OnNoSaveItemDialog);
                    return;
                }
            }

            m_SaveChoice.Choose();
        }

        // --------------------------------------------------------------------

        public void StartSave()
        {
            UIManager.Get<UISaveGame>().Show(m_LocationName, m_ItemRequiredToSave);
        }
    }
}