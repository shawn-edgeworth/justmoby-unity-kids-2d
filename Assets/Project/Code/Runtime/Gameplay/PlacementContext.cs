using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public readonly struct PlacementContext
    {
        public string ColorId { get; }
        public float CubeSize { get; }
        public Vector2 DropCenter { get; }

        public PlacementContext(string colorId, float cubeSize, Vector2 dropCenter)
        {
            ColorId = colorId;
            CubeSize = cubeSize;
            DropCenter = dropCenter;
        }
    }
}
