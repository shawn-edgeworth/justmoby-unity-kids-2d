using System;
using System.Collections.Generic;

namespace Assets.Project.Code.Runtime.Persistence
{
    [Serializable]
    public sealed class GameSaveData
    {
        public List<CubeSaveData> Cubes = new();
    }

    [Serializable]
    public sealed class CubeSaveData
    {
        public string ColorId;
        public float NormalizedCenterX;
        public float NormalizedSize;
    }
}
