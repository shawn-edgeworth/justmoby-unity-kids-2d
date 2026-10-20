using System.IO;
using Assets.Project.Code.Runtime.Persistence;
using UnityEditor;
using UnityEngine;

namespace Assets.Project.Code.Editor
{
    public static class EditorExtensions
    {
        [MenuItem("Saves/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            var folderPath = Application.persistentDataPath;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var saveFilePath = JsonSaveService.SaveFilePath;
            if (File.Exists(saveFilePath))
            {
                EditorUtility.RevealInFinder(saveFilePath);
                return;
            }

            EditorUtility.RevealInFinder(folderPath);
        }

        [MenuItem("Saves/Clear Save")]
        public static void ClearSave()
        {
            var saveFilePath = JsonSaveService.SaveFilePath;
            if (!File.Exists(saveFilePath))
            {
                Debug.Log($"Save file not found: {saveFilePath}");
                return;
            }

            File.Delete(saveFilePath);
            Debug.Log($"Save file deleted: {saveFilePath}");
        }
    }
}
