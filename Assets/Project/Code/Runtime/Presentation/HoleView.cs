using Coffee.UIExtensions;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    [ExecuteAlways]
    public sealed class HoleView : MonoBehaviour, IHoleArea
    {
        [field: SerializeField, Min(0f)] public float RadiusX { get; private set; } = 100f;
        [field: SerializeField, Min(0f)] public float RadiusY { get; private set; } = 50f;
        [field: SerializeField] public Vector2 CenterOffset { get; private set; }
        [field: SerializeField] public Color GizmoColor { get; private set; } = Color.white;

        [SerializeField] private UIParticle holeHoverVfx;
        [SerializeField] private UIParticle consumeVfx;

        private bool isHoleHoverActive;

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResetVfxState();
        }

        public bool Contains(Vector2 worldPoint)
        {
            if (RadiusX <= 0f || RadiusY <= 0f)
            {
                return false;
            }

            var localPoint = transform.InverseTransformPoint(worldPoint);
            var x = (localPoint.x - CenterOffset.x) / RadiusX;
            var y = (localPoint.y - CenterOffset.y) / RadiusY;
            return x * x + y * y <= 1f;
        }

        public void SetHoleHoverActive(bool isActive)
        {
            if (isHoleHoverActive == isActive)
            {
                return;
            }

            isHoleHoverActive = isActive;
            if (isActive)
            {
                holeHoverVfx.Play();
            }
            else
            {
                StopHoverVfx();
            }
        }

        public void PlayConsume()
        {
            consumeVfx.Stop();
            consumeVfx.Clear();
            consumeVfx.Play();
        }

#if UNITY_EDITOR
        public void SetData(float radiusX, float radiusY, Vector2 centerOffset)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
            CenterOffset = centerOffset;
        }
#endif

        private void ResetVfxState()
        {
            isHoleHoverActive = false;
            StopHoverVfx();
            consumeVfx.Stop();
            consumeVfx.Clear();
        }

        private void StopHoverVfx()
        {
            // UIParticle.Stop() only pauses simulation; Clear removes leftover particles.
            holeHoverVfx.Stop();
            holeHoverVfx.Clear();
        }
    }
}
