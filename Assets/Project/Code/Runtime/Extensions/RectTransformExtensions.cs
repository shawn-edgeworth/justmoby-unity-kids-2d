using UnityEngine;

namespace Assets.Project.Code.Runtime.Extensions
{
    public static class RectTransformExtensions
    {
        private static readonly Vector3[] Corners = new Vector3[4];

        public static Rect ToWorldRect(this RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(Corners);

            var min = (Vector2)Corners[0];
            var max = min;

            for (var i = 1; i < Corners.Length; i++)
            {
                min = Vector2.Min(min, Corners[i]);
                max = Vector2.Max(max, Corners[i]);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
