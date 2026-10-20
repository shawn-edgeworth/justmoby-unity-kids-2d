using System;
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEngine;

namespace Assets.Project.Code.Editor
{
    /// <summary>
    /// Applies gitignored OAuth credentials, pulls Google Sheets localization, then clears credentials from the asset.
    /// </summary>
    public static class LocalizationGoogleSheetsMenu
    {
        private const string CredentialsRelativePath = "Local/google-sheets-oauth.json";
        private const string ExampleRelativePath = "Local/google-sheets-oauth.example.json";
        private const string SheetsServiceAssetPath = "Assets/Localization/Settings/Google Sheets Service.asset";
        private const string StringTableCollectionName = "Main";

        [MenuItem("Localization/Pull From Google Sheets (local OAuth)")]
        public static void PullFromGoogleSheetsWithLocalOAuth()
        {
            var credentialsPath = GetProjectPath(CredentialsRelativePath);
            if (!File.Exists(credentialsPath))
            {
                Debug.LogError(
                    $"OAuth credentials file not found: '{credentialsPath}'. " +
                    $"Copy '{ExampleRelativePath}' to '{CredentialsRelativePath}' and fill clientId/clientSecret.");
                return;
            }

            var provider = AssetDatabase.LoadAssetAtPath<SheetsServiceProvider>(SheetsServiceAssetPath);
            if (provider == null)
            {
                throw new InvalidOperationException($"SheetsServiceProvider not found at '{SheetsServiceAssetPath}'.");
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(StringTableCollectionName);
            if (collection == null)
            {
                throw new InvalidOperationException(
                    $"StringTableCollection '{StringTableCollectionName}' was not found.");
            }

            GoogleSheetsExtension sheetsExtension = null;
            for (var i = 0; i < collection.Extensions.Count; i++)
            {
                if (collection.Extensions[i] is GoogleSheetsExtension googleExtension)
                {
                    sheetsExtension = googleExtension;
                    break;
                }
            }

            if (sheetsExtension == null)
            {
                throw new InvalidOperationException(
                    $"GoogleSheetsExtension is missing on StringTableCollection '{StringTableCollectionName}'.");
            }

            var credentials = LoadCredentials(credentialsPath);

            try
            {
                provider.SetOAuthCredentials(credentials.clientId, credentials.clientSecret);
                EditorUtility.SetDirty(provider);
                AssetDatabase.SaveAssetIfDirty(provider);

                // AuthorizeOAuth() returns Google UserCredential from Unity.Localization.ThirdParty.Editor;
                // calling it would require an asmdef reference. Pull uses provider.Service and triggers OAuth itself.
                var googleSheets = new GoogleSheets(provider)
                {
                    SpreadSheetId = sheetsExtension.SpreadsheetId
                };

                googleSheets.PullIntoStringTableCollection(
                    sheetsExtension.SheetId,
                    collection,
                    sheetsExtension.Columns,
                    sheetsExtension.RemoveMissingPulledKeys,
                    reporter: null,
                    createUndo: true);

                Debug.Log(
                    $"Pulled localization from Google Sheets into '{StringTableCollectionName}' " +
                    $"(spreadsheet '{sheetsExtension.SpreadsheetId}', sheet {sheetsExtension.SheetId}).");
            }
            finally
            {
                ClearCredentials(provider);
            }
        }

        [MenuItem("Localization/Reveal Local OAuth Credentials")]
        public static void RevealLocalOAuthCredentials()
        {
            var credentialsPath = GetProjectPath(CredentialsRelativePath);
            var folderPath = Path.GetDirectoryName(credentialsPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(credentialsPath))
            {
                var examplePath = GetProjectPath(ExampleRelativePath);
                if (File.Exists(examplePath))
                {
                    File.Copy(examplePath, credentialsPath);
                    Debug.Log($"Created '{CredentialsRelativePath}' from example. Fill in clientId/clientSecret.");
                }
                else
                {
                    File.WriteAllText(
                        credentialsPath,
                        "{\n  \"clientId\": \"\",\n  \"clientSecret\": \"\"\n}\n");
                    Debug.Log($"Created empty '{CredentialsRelativePath}'. Fill in clientId/clientSecret.");
                }
            }

            EditorUtility.RevealInFinder(credentialsPath);
        }

        private static OAuthCredentialsFile LoadCredentials(string path)
        {
            var json = File.ReadAllText(path);
            var credentials = JsonUtility.FromJson<OAuthCredentialsFile>(json);
            if (credentials == null)
            {
                throw new InvalidOperationException($"Failed to parse OAuth credentials JSON: '{path}'.");
            }

            if (string.IsNullOrWhiteSpace(credentials.clientId) ||
                string.IsNullOrWhiteSpace(credentials.clientSecret))
            {
                throw new InvalidOperationException(
                    $"OAuth credentials in '{path}' must contain non-empty clientId and clientSecret.");
            }

            return credentials;
        }

        private static void ClearCredentials(SheetsServiceProvider provider)
        {
            provider.SetOAuthCredentials(string.Empty, string.Empty);
            EditorUtility.SetDirty(provider);
            AssetDatabase.SaveAssetIfDirty(provider);
            AssetDatabase.SaveAssets();
            Debug.Log($"Cleared OAuth credentials from '{SheetsServiceAssetPath}'.");
        }

        private static string GetProjectPath(string relativePath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        [Serializable]
        private sealed class OAuthCredentialsFile
        {
            public string clientId;
            public string clientSecret;
        }
    }
}
