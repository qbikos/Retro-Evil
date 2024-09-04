using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public abstract class UISettingsElement : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI Label;
        public UnityEvent<string> OnValueChanged;

        protected SettingsElementContent m_Content;

        public virtual void Fill(SettingsElementContent content, SettingsManager settingsState)
        {
            m_Content = content;
            Label.text = content.Name;
        }
    }
}