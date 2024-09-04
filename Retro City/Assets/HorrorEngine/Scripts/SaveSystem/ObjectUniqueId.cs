using UnityEngine;

namespace HorrorEngine
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ObjectUniqueId : MonoBehaviour
    {
        [SerializeField] private string m_Id;
        [Tooltip("This indicates the Id doesn't need to be different from the prefab since there won't be more instances")]
        public bool IsUniqueInstance;

        public string Id => m_Id;

        // --------------------------------------------------------------------

        private void Awake()
        {
            if (string.IsNullOrEmpty(Id))
                RegenerateId();
        }

        // --------------------------------------------------------------------

        [ContextMenu("Regenerate Id")]
        public void RegenerateId()
        {
            m_Id = IdUtils.GenerateId();
        }

        // --------------------------------------------------------------------

        public void SetId(string id)
        {
            m_Id = id;
        }
    }
}