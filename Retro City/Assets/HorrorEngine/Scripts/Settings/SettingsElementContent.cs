using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    
    [Serializable]
    public abstract class SettingsElementContent : ScriptableObject
    {
        public string Name;
        public string SettingsKey;
        [FormerlySerializedAs("Prefab")]
        public GameObject UIPrefab;
        public SettingInitializationOrder Initialization = SettingInitializationOrder.Subsystem;

        public abstract void Apply();
        public abstract string GetDefaultValue();

        public T GetAsEnum<T>() where T : struct
        {
            SettingsManager.Instance.GetEnum<T>(this, out T outVal);
            return outVal;
        }

        public float GetAsFloat()
        {
            SettingsManager.Instance.GetFloat(this, out float outVal);
            return outVal;
        }

        public int GetAsInt()
        {
            SettingsManager.Instance.GetInt(this, out int outVal);
            return outVal;
        }
        public bool GetAsBool()
        {
            SettingsManager.Instance.GetBool(this, out bool outVal);
            return outVal;
        }
    }

}