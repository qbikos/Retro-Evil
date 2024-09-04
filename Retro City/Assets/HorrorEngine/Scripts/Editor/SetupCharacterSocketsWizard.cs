using UnityEngine;
using UnityEditor;

namespace HorrorEngine
{
    public class SetupCharacterSocketsWizard : EditorWindow
    {
        [SerializeField] private SocketHandle m_RightHandHandle;
        [SerializeField] private SocketHandle m_LeftHandHandle;
        [SerializeField] private SocketHandle m_RightFootHandle;
        [SerializeField] private SocketHandle m_LeftFootHandle;
        [SerializeField] private SocketHandle m_FlashlightHandle;

        private GameObject m_RHand;
        private GameObject m_LHand;
        private GameObject m_RFoot;
        private GameObject m_LFoot;
        private GameObject m_Flashlight;

        private int m_RHandPickerId;
        private int m_LHandPickerId;
        private int m_RFootPickerId;
        private int m_LFootPickerId;
        private int m_FlashlightId;

        [MenuItem("Horror Engine/Wizards/Setup Character Sockets")]
        static void Init()
        {
            SetupCharacterSocketsWizard window = (SetupCharacterSocketsWizard)EditorWindow.GetWindow(typeof(SetupCharacterSocketsWizard));
            window.titleContent = new GUIContent("Character Socket Setup");
            window.Show();
        }

        private void OnGUI()
        {
            ShowSocketEntry("Right Hand", ref m_RHand, ref m_RHandPickerId, 101, "Right Hand");
            ShowSocketEntry("Left Hand", ref m_LHand, ref m_LHandPickerId, 102, "Left Hand");
            ShowSocketEntry("Right Foot", ref m_RFoot, ref m_RFootPickerId, 103, "Right Foot");
            ShowSocketEntry("Left Foot", ref m_LFoot, ref m_LFootPickerId, 104, "Left Foot");
            ShowSocketEntry("Flashlight", ref m_Flashlight, ref m_FlashlightId, 105, "Flashlight");

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                int controlId = EditorGUIUtility.GetObjectPickerControlID();
                GameObject selectedGO = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                if (controlId == m_RHandPickerId) m_RHand = selectedGO;
                else if (controlId == m_LHandPickerId) m_LHand = selectedGO;
                else if (controlId == m_RFootPickerId) m_RFoot = selectedGO;
                else if (controlId == m_LFootPickerId) m_LFoot = selectedGO;
                else if (controlId == m_FlashlightId) m_Flashlight = selectedGO;
            }

            if (GUILayout.Button("Apply Sockets"))
            {
                AddSocket(m_RHand, m_RightHandHandle);
                AddSocket(m_LHand, m_LeftHandHandle);
                AddSocket(m_RFoot, m_RightFootHandle);
                AddSocket(m_LFoot, m_LeftFootHandle);
                AddSocket(m_Flashlight, m_FlashlightHandle);
            }
        }

        private void AddSocket(GameObject obj, SocketHandle handle)
        {
            if (!obj)
                return;

            Socket socket;
            if (!obj.TryGetComponent<Socket>(out socket))
            {
                socket = obj.AddComponent<Socket>();
            }

            socket.Handle = handle;
            EditorUtility.SetDirty(obj);
        }


        private void ShowSocketEntry(string label, ref GameObject obj, ref int controlID, int idOffset, string filter)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField(obj != null ? obj.name : "None", obj != null ? EditorStyles.objectField : EditorStyles.label);
            //EditorGUILayout.ObjectField(obj, typeof(GameObject), obj);
            if (GUILayout.Button("Pick", GUILayout.Width(50)))
            {
                controlID = EditorGUIUtility.GetControlID(FocusType.Passive) + idOffset;
                EditorGUIUtility.ShowObjectPicker<GameObject>(obj, true, filter, controlID);
            }
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                obj = null;
            }
            GUILayout.EndHorizontal();
        }
    }
}