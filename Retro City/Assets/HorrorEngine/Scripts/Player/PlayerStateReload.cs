using UnityEngine;

namespace HorrorEngine
{
    public class PlayerStateReload : ActorStateWithDuration
    {
     
        private AudioSource m_AudioSource;
        private InventoryEntry m_WeaponEntry;
        private ReloadableWeaponData m_Weapon;

        // --------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            m_AudioSource = GetComponentInParent<AudioSource>();
        }

        // --------------------------------------------------------------------

        public override void StateEnter(IActorState fromState)
        {
            m_WeaponEntry = GameManager.Instance.Inventory.GetEquippedWeapon();
            m_Weapon = m_WeaponEntry.Item as ReloadableWeaponData;
            Debug.Assert(m_Weapon, "ReloadableWeapon not equipped, it is assumed it'll be equipped in the primary equipment slot");

            m_Duration = m_Weapon.ReloadDuration;

            base.StateEnter(fromState);

            m_AudioSource.PlayOneShot(m_Weapon.ReloadSound);

            UIManager.Get<UIInputListener>().AddBlockingContext(this);

        }

        // --------------------------------------------------------------------

        public override void StateExit(IActorState intoState)
        {
            Reload();

            UIManager.Get<UIInputListener>().RemoveBlockingContext(this);

            base.StateExit(intoState);
        }

        // --------------------------------------------------------------------

        public virtual void Reload()
        {
            Debug.Assert(m_Weapon.AmmoItem != null, "Weapon can not reload, AmmoItem is null in the WeaponData");

            Inventory inventory = GameManager.Instance.Inventory;
            if (inventory.TryGet(m_Weapon.AmmoItem, out var ammoEntry))
                inventory.Combine(m_WeaponEntry, ammoEntry);
        }

    }
}