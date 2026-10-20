using System;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    [Serializable]
    public sealed class CubeColorConfig
    {
        [SerializeField] private string colorId;
        [SerializeField] private Color color = Color.white;

        public string ColorId => colorId;
        public Color Color => color;

        public CubeColorConfig()
        {
        }

        public CubeColorConfig(string colorId, Color color)
        {
            this.colorId = colorId;
            this.color = color;
        }
    }
}
