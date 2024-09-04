using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HorrorEngine
{

    public class SettingsSavedMessage : BaseMessage { }

    public enum SettingInitializationOrder
    {
        Subsystem,
        BeforeSplashScreen,
        AfterSceneLoad,
        DelayedAfterSceneLoad,
    }

    public class SettingsManager
    {
        private static readonly string k_PlayerPrefSettingsPrefix = "Settings.";
        private SettingsSet[] m_SettingsSets;
        private Dictionary<SettingsElementContent, string> m_Settings = new Dictionary<SettingsElementContent, string>();

        public GameSettingsDefaults Defaults;

        public static SettingsManager Instance { get; private set; }
        
        public void Set(SettingsElementContent Key, string Value)
        {
            m_Settings[Key] = Value;
        }

        public bool Get(SettingsElementContent Key, out string outValue)
        {
            return m_Settings.TryGetValue(Key, out outValue);
        }

        public bool GetInt(SettingsElementContent Key, out int outValue)
        {
            bool exists = Get(Key, out string outValueStr);
            if (!exists)
                outValueStr = Key.GetDefaultValue();
            outValue = Convert.ToInt32(outValueStr);
            return exists;
        }
        public bool GetBool(SettingsElementContent Key, out bool outValue)
        {
            bool exists = Get(Key, out string outValueStr);
            if (!exists)
                outValueStr = Key.GetDefaultValue();
            outValue = outValueStr == "1";
            return exists;
        }


        public bool GetEnum<T>(SettingsElementContent Key, out T outValue) where T : struct
        {
            bool exists = Get(Key, out string outValueStr);
            if (!exists)
                outValueStr = Key.GetDefaultValue();
            outValue = Enum.Parse<T>(outValueStr);
            return exists;
        }

        public bool GetFloat(SettingsElementContent Key, out float outValue)
        {
            bool exists = Get(Key, out string outValueStr);
            if (!exists)
                outValueStr = Key.GetDefaultValue();
            outValue = (float)Convert.ToDouble(outValueStr);
            return exists;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void LoadSubsystem()
        {
            Instance = new SettingsManager();
            Instance.m_SettingsSets = Resources.LoadAll<SettingsSet>("");
            Instance.Defaults = Resources.Load<GameSettingsDefaults>("DefaultSettings");

            Instance.Discard();
            Instance.ApplySaved(SettingInitializationOrder.Subsystem);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void LoadBeforeSplash()
        {
            Instance.ApplySaved(SettingInitializationOrder.BeforeSplashScreen);
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void LoadAfterSceneLoad()
        {
            Instance.ApplySaved(SettingInitializationOrder.AfterSceneLoad);
            Instance.DelayedApply();
        }

        // #HACK - This delay mode is a dirty hack needed to support AudioMixer parameter change after start
        async void DelayedApply() 
        {
            await Task.Delay(1);
            Instance.ApplySaved(SettingInitializationOrder.DelayedAfterSceneLoad);
        }

        public void ApplySaved(SettingInitializationOrder initializationOrder)
        {
            foreach(var pair in m_Settings)
            {
                if (pair.Key.Initialization == initializationOrder)
                    pair.Key.Apply();
            }
        }

        public void Discard()
        {
            m_Settings.Clear();

            foreach (var set in m_SettingsSets)
            {
                foreach (var content in set.Elements)
                {
                    string val = PlayerPrefs.GetString(k_PlayerPrefSettingsPrefix + content.SettingsKey);
                    if (!string.IsNullOrEmpty(val))
                    {
                        m_Settings.Add(content, val);
                    }
                }
            }
        }

        public void SaveAndApply()
        {
            foreach (var setting in m_Settings)
            {
                SettingsElementContent content = setting.Key;
                content.Apply();
                PlayerPrefs.SetString(k_PlayerPrefSettingsPrefix + content.SettingsKey, setting.Value);
            }

            MessageBuffer<SettingsSavedMessage>.Dispatch();
        }
    }

}