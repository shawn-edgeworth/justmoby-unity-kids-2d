using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Project.Code.Runtime.Presentation
{
    /// <summary>
    /// Gesture arbitration between ScrollRect scrolling and item drag-and-drop.
    /// Lives on the scroll view; wired via SceneRefs / Zenject into palette DragListeners.
    /// </summary>
    public sealed class ScrollDragGestureGate : MonoBehaviour
    {
        public enum Mode
        {
            Pending = 0,
            Scroll = 1,
            Drag = 2,
        }

        public enum Axis
        {
            Horizontal = 0,
            Vertical = 1,
        }

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float pixelThreshold = 12f;
        [SerializeField] private float timeThreshold = 0.2f;
        [SerializeField] private Axis scrollAxis = Axis.Horizontal;

        private Mode mode;
        private float beginTime;
        private bool scrollBeginForwarded;

        public Mode CurrentMode => mode;

        private void Awake()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void OnValidate()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (pixelThreshold < 0f)
            {
                pixelThreshold = 0f;
            }

            if (timeThreshold < 0f)
            {
                timeThreshold = 0f;
            }
        }

        /// <summary>
        /// Starts a new gesture classification cycle.
        /// </summary>
        public void Begin(PointerEventData eventData)
        {
            mode = Mode.Pending;
            beginTime = Time.unscaledTime;
            scrollBeginForwarded = false;
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
        }

        /// <summary>
        /// Classifies the gesture from pointer delta since begin.
        /// Once resolved, mode stays fixed until the next <see cref="Begin"/>.
        /// </summary>
        public Mode TryResolve(Vector2 deltaFromStart)
        {
            if (mode != Mode.Pending)
            {
                return mode;
            }

            var along = scrollAxis == Axis.Horizontal
                ? Mathf.Abs(deltaFromStart.x)
                : Mathf.Abs(deltaFromStart.y);
            var perp = scrollAxis == Axis.Horizontal
                ? Mathf.Abs(deltaFromStart.y)
                : Mathf.Abs(deltaFromStart.x);

            if (perp >= pixelThreshold)
            {
                mode = Mode.Drag;
                return mode;
            }

            if (along < pixelThreshold)
            {
                return mode;
            }

            var elapsed = Time.unscaledTime - beginTime;
            mode = elapsed < timeThreshold
                ? Mode.Scroll
                : Mode.Drag;
            return mode;
        }

        public void ForwardBeginDrag(PointerEventData eventData)
        {
            if (scrollBeginForwarded)
            {
                return;
            }

            scrollBeginForwarded = true;
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }

        public void ForwardDrag(PointerEventData eventData)
        {
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }

        public void ForwardEndDrag(PointerEventData eventData)
        {
            if (!scrollBeginForwarded)
            {
                return;
            }

            scrollBeginForwarded = false;
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        }

        public void ResetGesture()
        {
            mode = Mode.Pending;
            scrollBeginForwarded = false;
        }
    }
}
