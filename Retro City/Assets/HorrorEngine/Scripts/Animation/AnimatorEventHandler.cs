using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public class AnimatorEventHandler : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent<AnimationEvent> OnEvent;

        public void TriggerEvent(AnimationEvent e)
        {
            OnEvent?.Invoke(e);
        }
    }
}