using System;

namespace Assets.Project.Code.Runtime.Configuration.Json
{
    [Serializable]
    public sealed class JsonGameConfigData
    {
        public JsonCubeColorData[] paletteCubes;
    }

    [Serializable]
    public sealed class JsonCubeColorData
    {
        public string colorId;
        public string hex;
    }
}
