using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    [Serializable]
    public class ChoiceEntry
    {
        public string Text;
        public UnityEvent OnSelected;
    }

    [Serializable]
    public class ChoiceData
    {
        public DialogData ChoiceDialog;
        public ChoiceEntry[] Choices;

        [HideInInspector]
        [FormerlySerializedAs("Dialog")]
        public string[] Dialog_DEPRECATED;

        public bool IsValid()
        {
            return Choices.Length > 0;
        }   
    }


    public class Choice: MonoBehaviour
    {
        public ChoiceData Data;
        public UnityEvent OnChoiceStart;
        public UnityEvent OnChoiceEnd;

        private UnityAction m_OnClose;

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_OnClose = OnClose;
        }

        // --------------------------------------------------------------------

        public void Choose()
        {
            OnChoiceStart?.Invoke();

            UIManager.PushAction(new UIStackedAction()
            {
                Action = m_OnClose,
                Name = "Choice.Choose (OnClose)"
            });

            UIManager.Get<UIChoices>().Show(Data);
        }

        // --------------------------------------------------------------------

        private void OnClose()
        {
            OnChoiceEnd?.Invoke();
        }


        // --------------------------------------------------------------------

        public void Cancel()
        {
            OnChoiceEnd = null;
            UIChoices choices = UIManager.Get<UIChoices>();
            if (choices.isActiveAndEnabled)
                choices.Hide();
        }
    }
}