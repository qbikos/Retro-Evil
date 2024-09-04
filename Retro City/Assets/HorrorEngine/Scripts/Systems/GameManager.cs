using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HorrorEngine
{
    [Serializable]
    public class GameSaveData
    {
        public static readonly int k_CurrentVersion = 1; // Increase this number to force player save data clearance
        
        public int Version;
        public string Date;
        public int SaveCount;
        public string SaveLocation;
        public string SceneName;
        public string CharacterName;
        public string CharacterId;
        public List<CharacterStateSaveData> CharacterStates;
        public ContainerSaveData SharedStorageBox;
    }

    public class GameOverMessage : BaseMessage { }

   

    public class GameManager : SingletonBehaviour<GameManager>, ISavable<GameSaveData>
    {
        public static readonly string k_StartCharacterPlayerPrefs = "StartCharacter";

        [HideInInspector]
        public PlayerActor Player;
        public List<CharacterData> Characters;
        public InventorySetup InventorySetup;
        [Tooltip("The storage box items will be shared betwen characters")]
        public bool ShareStorageBox;
        [ShowIf("ShareStorageBox")]
        public ContainerData SharedStorageBox;

        [Header("Databases")]
        public ItemDatabase ItemDatabase;
        public DocumentDatabase DocumentDatabase;
        public MapDatabase MapDatabase;
        public SpawnableSavableDatabase SpawnableDatabase;

        [HideInInspector]
        public UnityEvent<PlayerActor> OnPlayerRegistered;

        private bool m_IsPlaying;
        private List<CharacterState> m_CharacterStates = new List<CharacterState>();
        private CharacterState m_SelectedCharacterState;
        private ContainerData m_CurrentSharedStorageBox;
        private int m_SaveCount;

        public Inventory Inventory => m_SelectedCharacterState.Inventory;
        public ContainerData StorageBox => ShareStorageBox ? m_CurrentSharedStorageBox : m_SelectedCharacterState.StorageBox;

        // --------------------------------------------------------------------

        public bool IsPlaying
        { 
            get
            {
                return !PauseController.Instance.IsPaused && m_IsPlaying;
            }
            set
            {
                m_IsPlaying = value;
            }
        }

        // --------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            InitializeRegisters();
            InitializeCharacters();

            if (ShareStorageBox)
            {
                m_CurrentSharedStorageBox = new ContainerData();
                m_CurrentSharedStorageBox.Copy(SharedStorageBox);
                m_CurrentSharedStorageBox.FillCapacityWithEmptyEntries();
            }

            MessageBuffer<SceneTransitionPreMessage>.Subscribe(OnSceneTransitionPre);
            MessageBuffer<SceneTransitionPostMessage>.Subscribe(OnSceneTransitionPost);
            MessageBuffer<DoorTransitionMidWayMessage>.Subscribe(OnDoorTransitionMidway);
        }

        // --------------------------------------------------------------------

        private void InitializeCharacters()
        {
            Debug.Assert(Characters.Count > 0, "The character list is empty");

            PlayerSpawnPoint spawnPoint = FindObjectOfType<PlayerSpawnPoint>();

            CharacterData startCharacter = spawnPoint.Character ? spawnPoint.Character : Characters[0];
            string startCharacterId = PlayerPrefs.GetString(k_StartCharacterPlayerPrefs, startCharacter.UniqueId);

            bool spawned = false;
            foreach (var characterData in Characters)
            {
                CharacterState characterState = new CharacterState(characterData);
                m_CharacterStates.Add(characterState);

                if (characterData.UniqueId == startCharacterId)
                {
                    SpawnCharacter(characterData, spawnPoint.transform, true);
                    spawned = true;
                }
            }

            if (!spawned)
            {
                Debug.LogError($"Character to spawn couldn't be found in character list. Expected character Id {startCharacterId}");
                SpawnCharacter(Characters[0], spawnPoint.transform, true);
            }

            PlayerPrefs.DeleteKey(k_StartCharacterPlayerPrefs);
        }

        // --------------------------------------------------------------------

        private CharacterData GetCharacterById(string id)
        {
            foreach (var character in Characters)
            {
                if (id == character.UniqueId)
                {
                    return character;
                }
            }

            Debug.LogError("Character Id could not be found");
            return null;
        }

        // --------------------------------------------------------------------

        private void OnDestroy()
        {
            MessageBuffer<SceneTransitionPreMessage>.Unsubscribe(OnSceneTransitionPre);
            MessageBuffer<SceneTransitionPostMessage>.Unsubscribe(OnSceneTransitionPost);
            MessageBuffer<DoorTransitionMidWayMessage>.Unsubscribe(OnDoorTransitionMidway);
        }

        // --------------------------------------------------------------------

        private void SpawnCharacter(CharacterData character, Transform spawnPoint, bool initialState)
        {
            Debug.Log($"Spawning character {character}");
            var playerPoolable = GameObjectPool.Instance.GetFromPool(character.Prefab, transform.parent);
            PlayerActor player = playerPoolable.GetComponent<PlayerActor>();
            player.Character = character;
            
            SetCurrentPlayer(player);
            
            if (initialState)
                m_SelectedCharacterState.SetupInitialEquipment(player);
            
            if (spawnPoint)
            {
                player.PlaceAt(spawnPoint);
                //player.GetComponent<GameObjectReset>().ResetComponents(); // This is to reset the animSpeedSetter, but might be overkill
            }

            player.gameObject.SetActive(true);
        }

        // --------------------------------------------------------------------

        public void SetCurrentPlayer(PlayerActor player)
        {
            if (Player)
            {
                Player.GetComponent<Health>().OnDeath.RemoveListener(OnPlayerDeath);
            }

            Player = player;
            Player.GetComponent<Health>().OnDeath.AddListener(OnPlayerDeath);

            SetSelectCharacterState(player.Character);

            OnPlayerRegistered?.Invoke(player);
        }

        // --------------------------------------------------------------------

        private void SetSelectCharacterState(CharacterData selected)
        {
            foreach (var characterState in m_CharacterStates) 
            {
                if (characterState.Data == selected) 
                {
                    m_SelectedCharacterState = characterState;
                    return;
                }
            }

            Debug.LogError("Selected character couldn't be found in the character list");
        }

        // --------------------------------------------------------------------

        private void Start()
        {
            m_IsPlaying = true;
        }

        // --------------------------------------------------------------------

        public void InitializeRegisters()
        {
            ItemDatabase.HashRegisters();
            DocumentDatabase.HashRegisters();
            MapDatabase.HashRegisters();
        }

        // --------------------------------------------------------------------

        void OnDoorTransitionMidway(DoorTransitionMidWayMessage msg)
        {
            ObjectStateManager.Instance.CaptureStates();
        }

        // --------------------------------------------------------------------

        void OnSceneTransitionPre(SceneTransitionPreMessage msg)
        {
            ObjectStateManager.Instance.CaptureStates();
        }

        // --------------------------------------------------------------------

        void OnSceneTransitionPost(SceneTransitionPostMessage msg)
        {
            ObjectStateManager.Instance.InstantiateSpawned(SceneManager.GetActiveScene(), SpawnableDatabase);
            ObjectStateManager.Instance.ApplyStates();
        }

        // --------------------------------------------------------------------

        void OnPlayerDeath(Health health)
        {
            MessageBuffer<GameOverMessage>.Dispatch();
            IsPlaying = false;   
        }

        // --------------------------------------------------------------------

        public void IncreaseSaveCount()
        {
            ++m_SaveCount;
        }

        // --------------------------------------------------------------------

        public void SwitchCharacter(CharacterData character, Transform spawnPoint)
        {
            Debug.Log($"Switching character {character}");

            if (character != m_SelectedCharacterState.Data)
            {
                if (Player)
                {
                    var pooled = Player.GetComponent<PooledGameObject>();
                    Debug.Assert(pooled, "Player should have a PooledGameObject component");
                    pooled?.ReturnToPool();
                }

                SpawnCharacter(character, spawnPoint, false);
            }
            else
            {
                if (spawnPoint)
                    Player.PlaceAt(spawnPoint);
            }
        }

        // --------------------------------------------------------------------

        public void Clear()
        {
            foreach(var character in m_CharacterStates)
            {
                character.Clear();
            }

            if (ShareStorageBox)
                m_CurrentSharedStorageBox.Clear();
        }

        //------------------------------------------------------
        // ISavable implementation
        //------------------------------------------------------

        public GameSaveData GetSavableData()
        {
            GameSaveData savedData = new GameSaveData();
            savedData.Version = GameSaveData.k_CurrentVersion;
            savedData.Date = DateTime.Now.ToString();
            savedData.SaveCount = m_SaveCount;
            savedData.SceneName = SceneManager.GetActiveScene().name;
            savedData.CharacterStates = new List<CharacterStateSaveData>();
            savedData.CharacterName = m_SelectedCharacterState.Data.Name;
            savedData.CharacterId = m_SelectedCharacterState.Data.UniqueId;

            foreach (var characterState in m_CharacterStates)
            {
                savedData.CharacterStates.Add(characterState.GetSavableData());
            }

            if (ShareStorageBox)
                savedData.SharedStorageBox = m_CurrentSharedStorageBox.GetSavableData();

            return savedData;
        }

        public void SetFromSavedData(GameSaveData savedData)
        {
            m_SaveCount = savedData.SaveCount;

            var character = GetCharacterById(savedData.CharacterId);
            SwitchCharacter(character, null);
            
            foreach (var savedCharacterState in savedData.CharacterStates)
            {
                foreach (var characterState in m_CharacterStates)
                {
                    if (savedCharacterState.HandleId == characterState.Data.UniqueId)
                    {
                        characterState.SetFromSavedData(savedCharacterState);
                        break;
                    }
                }
            }

            if (ShareStorageBox)
                m_CurrentSharedStorageBox.SetFromSavedData(savedData.SharedStorageBox);
        }


    }
}