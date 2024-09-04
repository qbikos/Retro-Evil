using UnityEngine;

namespace HorrorEngine 
{
    public class ObjectInstantiator : MonoBehaviour
    {
        [SerializeField] private GameObject Prefab;
        [SerializeField] private ObjectInstantiationSettings Settings;

        private SocketController m_SocketController;

        private void Awake()
        {
            m_SocketController = GetComponentInParent<SocketController>();
        }

        public GameObject Instatiate(GameObject prefab = null)
        {
            if (prefab)
                Prefab = prefab;

            if (Prefab)
            {
                return Settings.Instantiate(Prefab, m_SocketController);
            }

            return null;
        }
    }
}