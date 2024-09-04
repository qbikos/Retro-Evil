using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace HorrorEngine
{
    public class HorrorEngineScripts
    {
        [MenuItem("Horror Engine/Scripts/Fix DialogData")]
        public static void FixDialogs()
        {
            var choices = GameObject.FindObjectsOfType<Choice>();
            foreach(var c in choices)
            {
                c.Data.ChoiceDialog = ConvertArrayToDialog(c.Data.Dialog_DEPRECATED);
                EditorUtility.SetDirty(c);
            }

            var dooLocks = GameObject.FindObjectsOfType<DoorLock>();
            foreach (var dl in dooLocks)
            {
                ConvertPrivateDialog(dl, "m_LockedDialog_DEPRECATED", "m_OnLockedDialog");
                EditorUtility.SetDirty(dl);
            }

            var doorLockKeys = GameObject.FindObjectsOfType<DoorLockKeyItem>();
            foreach (var dlk in doorLockKeys)
            {
                ConvertPrivateDialog(dlk, "m_LockedOtherSideDialog_DEPRECATED", "m_OnLockedOtherSideDialog");
                ConvertPrivateDialog(dlk, "m_OnUnlockDialog_DEPRECATED", "m_OnUnlockedDialog");
                EditorUtility.SetDirty(dlk);
            }

            var doorOtherSide = GameObject.FindObjectsOfType<DoorLockOtherSide>();
            foreach (var dos in doorOtherSide)
            {
                ConvertPrivateDialog(dos, "m_OnUnlockDialog_DEPRECATED", "m_OnUnlockedDialog");
                EditorUtility.SetDirty(dos);
            }

            var pointsOfInteres = GameObject.FindObjectsOfType<PointOfInterest>();
            foreach (var poi in pointsOfInteres)
            {
                ConvertPrivateDialog(poi, "Dialog_DEPRECATED", "m_Dialog");
                EditorUtility.SetDirty(poi);
            }

            var intWithItemUse = GameObject.FindObjectsOfType<InteractiveWithItemUse>();
            foreach (var iwu in intWithItemUse)
            {
                ConvertPrivateDialog(iwu, "m_NoItemDialog_DEPRECATED", "m_OnNoItemDialog");
                EditorUtility.SetDirty(iwu);
            }


        }

        private static void ConvertPrivateDialog(object obj, string oldVar, string newVar)
        {
            System.Type typ = obj.GetType();
            FieldInfo newField = typ.GetField(newVar, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo oldField = typ.GetField(oldVar, BindingFlags.NonPublic | BindingFlags.Instance);
            string[] oldDialog = (string[])oldField.GetValue(obj);
            DialogData dialogData = ConvertArrayToDialog(oldDialog);
            newField.SetValue(obj, dialogData);
        }

        private static DialogData ConvertArrayToDialog(string[] array)
        {
            DialogData data = new DialogData();
            if (array == null || array.Length == 0)
                return data;

            DialogLine[] lines = new DialogLine[array.Length];
            for(int i = 0; i < array.Length; ++i)
            {
                lines[i] = new DialogLine();
                lines[i].Text = array[i];
            }
            data.SetLines(lines);

            return data;
        }


        [MenuItem("Horror Engine/Scripts/Fix Documents")]
        public static void FixDocuments()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(DocumentData).Name);
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DocumentData doc = (DocumentData)AssetDatabase.LoadAssetAtPath(path, typeof(DocumentData));

                if (doc.Pages.Length > 0)
                {
                    Debug.LogWarning($"Document {doc.name} already has pages setup. Skipping document. Remove the pages before updating", doc);
                    continue;
                }

                doc.Pages = new DocumentPage[doc.PagesText_DEPRECATED.Length];
                int index = 0;
                foreach(var pageText in doc.PagesText_DEPRECATED)
                {
                    doc.Pages[index] = new DocumentPage()
                    {
                        Text = pageText
                    };
                    ++index;
                }

                EditorUtility.SetDirty(doc);
                AssetDatabase.SaveAssetIfDirty(doc);
            }

        }

    }
}