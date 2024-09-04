using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class CharacterMessage : BaseMessage
    {
        public CharacterData Character;
    }

    [Serializable]
    public struct CharacterStateSaveData
    {
        public string HandleId;
        public InventorySaveData Inventory;
        public ContainerSaveData StorageBox;
    }

    [Serializable]
    public class CharacterState : ISavable<CharacterStateSaveData>
    {
        public Inventory Inventory = new Inventory();
        public ContainerData StorageBox; // TODO - Add an option to share this between players?

        private CharacterData m_Data;

        public CharacterData Data => m_Data;

        // --------------------------------------------------------------------

        public CharacterState(CharacterData data)
        {
            m_Data = data;

            Inventory.Init(data);

            StorageBox = new ContainerData();
            StorageBox.Copy(data.InitialStorageBox);
            StorageBox.FillCapacityWithEmptyEntries();
        }

        // --------------------------------------------------------------------

        public void SetupInitialEquipment(PlayerActor player)
        {
            PlayerEquipment equipment = player.GetComponentInChildren<PlayerEquipment>();
            foreach (var e in m_Data.InitialEquipment)
            {
                if (Inventory.TryGet(e, out InventoryEntry entry))
                    Inventory.Equip(entry);
                else
                    equipment.Equip(e, e.Slot);
            }
        }

        // --------------------------------------------------------------------

        public void Clear()
        {
            StorageBox.Clear();
            Inventory.Clear();
        }

        // --------------------------------------------------------------------

        public CharacterStateSaveData GetSavableData()
        {
            CharacterStateSaveData characterSavedData = new CharacterStateSaveData();
            characterSavedData.HandleId = m_Data.UniqueId;
            characterSavedData.Inventory = Inventory.GetSavableData();
            characterSavedData.StorageBox = StorageBox.GetSavableData();
            return characterSavedData;
        }

        public void SetFromSavedData(CharacterStateSaveData savedData)
        {
            Inventory.SetFromSavedData(savedData.Inventory);
            StorageBox.SetFromSavedData(savedData.StorageBox);
        }

    }

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Horror Engine/Character", order = -1)]
    public class CharacterData : Register
    {
        public string Name;
        public GameObject Prefab;
        
        [Header("Initial State")]
        public InventoryEntry[] InitialInventory;
        public ContainerData InitialStorageBox;
        public DocumentData[] InitialDocuments;
        public MapData[] InitialMaps;
        public EquipableItemData[] InitialEquipment;
    }
}