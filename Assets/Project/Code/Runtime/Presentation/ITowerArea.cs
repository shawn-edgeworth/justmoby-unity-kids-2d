using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    public interface ITowerArea
    {
        bool Contains(Vector2 worldPoint);
        float BottomY { get; }
        float TopY { get; }
        float MinX { get; }
        float MaxX { get; }
        float CenterX { get; }
        float ClampBaseCenterX(float centerX, float cubeSize);
        float ResolveStackedCenterX(float anchorX, float offset, float cubeSize);
    }
}
