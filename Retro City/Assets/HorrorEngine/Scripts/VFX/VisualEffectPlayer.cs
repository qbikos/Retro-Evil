using UnityEngine;

namespace HorrorEngine
{
    public abstract class VFXSelector : ScriptableObject
    {
        [SerializeField] VFXSelector m_Prototype;
        public virtual GameObject Select(VisualEffectPlayer player)
        {
            return m_Prototype ? m_Prototype.Select(player) : null;
        }
    }

    [System.Serializable]
    public class VFXEntry
    {
        public string Identifier;
        public VFXSelector Selector;
        public GameObject Effect;
        public ObjectInstantiationSettings InstantiationSettings;
    }

    public class VisualEffectPlayer : MonoBehaviour
    {
        [SerializeField] private SocketController m_SocketController;
        [SerializeField] private VFXEntry[] m_VisualEffects;
       
        public void PlayVFX(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.5f)
            {
                foreach (var vfx in m_VisualEffects)
                {
                    if (vfx.Identifier == evt.stringParameter)
                    {
                        GameObject prefab;
                        if (vfx.Selector)
                            prefab = vfx.Selector.Select(this);
                        else
                            prefab = vfx.Effect;

                        if (prefab)
                            vfx.InstantiationSettings.Instantiate(prefab, m_SocketController);
                    }
                }
            }
        }

    }
}