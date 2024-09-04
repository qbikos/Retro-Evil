using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
 
    public class UISettingsList : MonoBehaviour
    {
        [SerializeField] SettingsSet Settings;

        private void Start()
        {
            foreach (SettingsElementContent content in Settings.Elements)
            {
                Debug.Assert(content, "A setting element does not have any assigned content");
                if (content)
                {
                    Debug.Assert(content.UIPrefab, "A setting element content does not have any assigned prefab");
                    if (content.UIPrefab)
                    {
                        GameObject instance = Instantiate(content.UIPrefab, transform);
                        UISettingsElement uiElement = instance.GetComponent<UISettingsElement>();
                        uiElement.Fill(content, SettingsManager.Instance);
                        uiElement.OnValueChanged.AddListener((newValue) =>
                        {
                            SettingsManager.Instance.Set(content, newValue);
                        });
                    }
                }

                gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        }
    }
}