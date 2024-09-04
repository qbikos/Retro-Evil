using System;
using UnityEngine;

namespace HorrorEngine
{
    [Flags]
    public enum InventoryMainAction
    {
        None,
        Use,
        Equip
    }

    [Flags]
    public enum ItemFlags
    {
        Stackable           = 1 << 1,
        ConsumeOnUse        = 1 << 2,
        Combinable          = 1 << 3,
        Examinable          = 1 << 4,
        Droppable           = 1 << 5,
        CreatePickupOnDrop  = 1 << 6,
        UseOnInteractive    = 1 << 7,
        Depletable          = 1 << 8,
    }

    [CreateAssetMenu(menuName = "Horror Engine/Items/Item")]
    public class ItemData : Register
    {
        public Sprite Image;
        public GameObject ExamineModel;
        public string Name;
        public string Description;
        public InventoryMainAction InventoryAction = InventoryMainAction.Use;
        public ItemFlags Flags;
        public SpawnableSavable DropPrefab;
        [ShowIf("Flags", Op.BitmaskContain, ItemFlags.Stackable)]
        public int MaxStackSize;

        private void OnEnable()
        {
            ItemOverride[] itemOverrides = Resources.LoadAll<ItemOverride>("");
            foreach (var iOverride in itemOverrides)
            {
                if (iOverride.Item == this && iOverride.ApplyOnAwake)
                {
                    foreach (ItemOverrideData propOverride in iOverride.Overrides)
                    {
                        propOverride.Override(this);
                    }
                }
            }   
        }

        public virtual void OnUse(InventoryEntry entry) { }
    }
}