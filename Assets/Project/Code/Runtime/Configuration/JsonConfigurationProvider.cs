using System;
using System.Collections.Generic;
using System.IO;
using Assets.Project.Code.Runtime.Configuration.Json;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Project.Code.Runtime.Configuration
{
    public sealed class JsonConfigurationProvider : IGameConfigProvider
    {
        public const string Address = "game_config";

        private readonly List<CubeColorConfig> paletteCubes;

        public JsonConfigurationProvider()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(Address);
            var textAsset = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded || textAsset == null)
            {
                var error = handle.OperationException != null
                    ? handle.OperationException.Message
                    : "unknown error";
                Addressables.Release(handle);
                throw new FileNotFoundException(
                    $"Game config JSON was not found at Addressables address '{Address}': {error}.",
                    Address);
            }

            try
            {
                var json = textAsset.text;
                var data = JsonConvert.DeserializeObject<JsonGameConfigData>(json);
                if (data == null || data.paletteCubes == null)
                {
                    throw new InvalidOperationException($"Game config JSON is invalid: '{Address}'.");
                }

                paletteCubes = new List<CubeColorConfig>(data.paletteCubes.Length);
                for (var i = 0; i < data.paletteCubes.Length; i++)
                {
                    var entry = data.paletteCubes[i];
                    paletteCubes.Add(new CubeColorConfig(entry.colorId, ParseHexColor(entry.hex, entry.colorId)));
                }
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        public IReadOnlyList<CubeColorConfig> GetPaletteCubes()
        {
            return paletteCubes;
        }

        public bool TryGetColor(string colorId, out Color color)
        {
            for (var i = 0; i < paletteCubes.Count; i++)
            {
                if (paletteCubes[i].ColorId == colorId)
                {
                    color = paletteCubes[i].Color;
                    return true;
                }
            }

            color = default;
            return false;
        }

        private static Color ParseHexColor(string hex, string colorId)
        {
            if (string.IsNullOrEmpty(hex))
            {
                throw new InvalidOperationException($"Color '{colorId}' has empty hex.");
            }

            var html = hex[0] == '#' ? hex : "#" + hex;
            if (!ColorUtility.TryParseHtmlString(html, out var color))
            {
                throw new InvalidOperationException($"Color '{colorId}' has invalid hex '{hex}'.");
            }

            return color;
        }
    }
}
