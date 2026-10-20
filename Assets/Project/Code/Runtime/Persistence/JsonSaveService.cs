using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Persistence
{
    public sealed class JsonSaveService : ISaveService
    {
        public const string FileName = "GameSave.json";

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);

        public bool TryLoad(out GameSaveData data)
        {
            data = null;
            var path = SaveFilePath;

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }

                data = JsonConvert.DeserializeObject<GameSaveData>(json);
                return data != null && data.Cubes != null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                data = null;
                return false;
            }
        }

        public void Save(GameSaveData data)
        {
            var path = SaveFilePath;
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }
    }
}
