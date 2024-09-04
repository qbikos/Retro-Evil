using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class AnimatorOverrider : MonoBehaviour, IResetable
    {
        private Animator m_Animator;

        private AnimatorOverrideController m_OverrideController;

        private Dictionary<AnimationClip, AnimationClip> m_OverridesDictionary = new Dictionary<AnimationClip, AnimationClip>();
        private List<AnimatorOverrideController> m_AppliedControllers = new List<AnimatorOverrideController>();

        List<KeyValuePair<AnimationClip, AnimationClip>> m_ClearOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        List<KeyValuePair<AnimationClip, AnimationClip>> m_NewOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();

            m_OverrideController = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
            m_Animator.runtimeAnimatorController = m_OverrideController;
            m_OverrideController.GetOverrides(m_NewOverrides);

            
            foreach (var anim in m_OverrideController.animationClips)
            {
                m_ClearOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(anim, null));
            }
        }

        // --------------------------------------------------------------------

        private void ReapplyAllOverrides()
        {
            m_OverridesDictionary.Clear();

            MapOverrides(m_ClearOverrides);
            m_OverrideController.ApplyOverrides(m_ClearOverrides);

            foreach (var overrideCtrl in m_AppliedControllers)
            {
                m_NewOverrides.Clear();
                overrideCtrl.GetOverrides(m_NewOverrides);
                MapOverrides(m_NewOverrides);
            }

            m_NewOverrides.Clear();
            
            foreach (var overrideEntry in m_OverridesDictionary)
            {
                m_NewOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(overrideEntry.Key, overrideEntry.Value));
            }

            m_OverrideController.ApplyOverrides(m_NewOverrides);
        }

        // --------------------------------------------------------------------

        private void MapOverrides(List<KeyValuePair<AnimationClip, AnimationClip>> overrideList)
        {
            foreach (var pair in overrideList)
            {
                if (m_OverridesDictionary.ContainsKey(pair.Key))
                {
                    m_OverridesDictionary[pair.Key] = pair.Value;
                }
                else
                {
                    m_OverridesDictionary.Add(pair.Key, pair.Value);
                }
            }
        }

        // --------------------------------------------------------------------

        public void RemoveOverride(AnimatorOverrideController overrideCtrl)
        {
            if (m_AppliedControllers.Remove(overrideCtrl))
            {
                ReapplyAllOverrides();
            }
            else
            {
                Debug.LogError($"AnimationOverrideController {overrideCtrl} was not applied");
            }
        }

        // --------------------------------------------------------------------

        public void AddOverride(AnimatorOverrideController overrideCtrl)
        {
            if (!m_AppliedControllers.Contains(overrideCtrl))
            {
                m_AppliedControllers.Add(overrideCtrl);
                ReapplyAllOverrides();
            }
            else
            {
                Debug.LogError($"AnimationOverrideController {overrideCtrl} was already applied");
            }

        }

        public void OnReset()
        {
            m_AppliedControllers.Clear();
            ReapplyAllOverrides();
        }
    }
}