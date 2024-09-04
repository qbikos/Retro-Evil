using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    [CreateAssetMenu(fileName = "InteractionData", menuName = "Horror Engine/Interaction")]
    public class InteractionData : ScriptableObject
    {
        public string Prompt;
        public Sprite Icon;
        public AnimatorStateHandle AnimState;
        [FormerlySerializedAs("InteractionTime")]
        public float InteractionDuration;
        public float InteractionDelay;
        public bool RotateToInteractive = true;

        private void OnValidate()
        {
            if (InteractionDuration < InteractionDelay)
                InteractionDuration = InteractionDelay;
        }
    }
}