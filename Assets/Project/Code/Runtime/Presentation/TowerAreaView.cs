using Assets.Project.Code.Runtime.Extensions;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class TowerAreaView : MonoBehaviour, ITowerArea
    {
        [SerializeField] private RectTransform rectTransform;

        public float BottomY => rectTransform.ToWorldRect().yMin;

        public float TopY => rectTransform.ToWorldRect().yMax;

        public float MinX => rectTransform.ToWorldRect().xMin;

        public float MaxX => rectTransform.ToWorldRect().xMax;

        public float CenterX => rectTransform.ToWorldRect().center.x;

        public bool Contains(Vector2 worldPoint)
        {
            return rectTransform.ToWorldRect().Contains(worldPoint);
        }

        public float ClampBaseCenterX(float centerX, float cubeSize)
        {
            var halfSize = cubeSize * 0.5f;
            return Mathf.Clamp(centerX, MinX + halfSize, MaxX - halfSize);
        }

        public float ResolveStackedCenterX(float anchorX, float offset, float cubeSize)
        {
            var halfSize = cubeSize * 0.5f;
            var minCenterX = MinX + halfSize;
            var maxCenterX = MaxX - halfSize;
            var proposedX = anchorX + offset;
            var flippedX = anchorX - offset;

            if (proposedX >= minCenterX && proposedX <= maxCenterX)
            {
                return proposedX;
            }

            if (flippedX >= minCenterX && flippedX <= maxCenterX)
            {
                return flippedX;
            }

            return Mathf.Clamp(proposedX, minCenterX, maxCenterX);
        }
    }
}
