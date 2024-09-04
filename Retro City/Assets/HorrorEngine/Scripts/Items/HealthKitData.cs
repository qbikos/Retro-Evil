using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    [CreateAssetMenu(menuName = "Horror Engine/Items/HealthKit")]
    public class HealthKitData : ItemData
    {
        [SerializeField] private float m_Regeneration;
        [SerializeField] private bool m_CompleteRegeneration;

        public override void OnUse(InventoryEntry entry)
        {
            base.OnUse(entry);

            if (m_CompleteRegeneration)
                GameManager.Instance.Player.GetComponent<Health>().RegenerateAll();
            else
                GameManager.Instance.Player.GetComponent<Health>().Regenerate(m_Regeneration);
        }
    }
}