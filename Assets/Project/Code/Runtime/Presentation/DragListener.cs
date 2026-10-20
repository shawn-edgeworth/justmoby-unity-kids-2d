using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class DragListener : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        ICancelHandler
    {
        /// <summary>
        /// Standalone (no gesture gate) only: min pointer travel in px before BeginDrag event fires.
        /// Used by tower cubes. Palette uses <see cref="ScrollDragGestureGate"/> thresholds instead.
        /// </summary>
        [SerializeField] private float dragThreshold = 10f;

        private ScrollDragGestureGate gestureGate;
        private bool isGestureArmed;
        private bool isDragging;
        private Vector2 beginPosition;
        private bool endHandled;
        private PointerEventData activeEventData;

        public event Action<DragListener, Vector2> BeginDrag;
        public event Action<DragListener, Vector2> Drag;
        public event Action<DragListener, Vector2> EndDrag;

        /// <summary>
        /// Binds palette scroll arbitration. Tower cubes leave this unset.
        /// </summary>
        public void SetGestureGate(ScrollDragGestureGate gate)
        {
            gestureGate = gate;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = false;
            endHandled = false;
            isGestureArmed = true;
            beginPosition = eventData.position;
            activeEventData = eventData;

            if (gestureGate != null)
            {
                gestureGate.Begin(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (endHandled)
            {
                return;
            }

            CompleteDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            activeEventData = eventData;

            if (!isGestureArmed)
            {
                isDragging = false;
                endHandled = false;
                isGestureArmed = true;
                beginPosition = eventData.position;

                if (gestureGate != null)
                {
                    gestureGate.Begin(eventData);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (endHandled)
            {
                return;
            }

            activeEventData = eventData;

            if (gestureGate != null)
            {
                HandleGatedDrag(eventData);
                return;
            }

            HandleStandaloneDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CompleteDrag(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            CompleteDrag(activeEventData);
        }

        private void OnDisable()
        {
            if (isDragging || (gestureGate != null && gestureGate.CurrentMode == ScrollDragGestureGate.Mode.Scroll))
            {
                CompleteDrag(activeEventData);
                return;
            }

            isDragging = false;
            isGestureArmed = false;
            if (gestureGate != null)
            {
                gestureGate.ResetGesture();
            }
        }

        private void HandleGatedDrag(PointerEventData eventData)
        {
            var mode = gestureGate.CurrentMode;
            if (mode == ScrollDragGestureGate.Mode.Pending)
            {
                mode = gestureGate.TryResolve(eventData.position - beginPosition);
            }

            switch (mode)
            {
                case ScrollDragGestureGate.Mode.Pending:
                    return;

                case ScrollDragGestureGate.Mode.Scroll:
                    gestureGate.ForwardBeginDrag(eventData);
                    gestureGate.ForwardDrag(eventData);
                    return;

                case ScrollDragGestureGate.Mode.Drag:
                    if (!isDragging)
                    {
                        isDragging = true;
                        BeginDrag?.Invoke(this, eventData.position);
                    }

                    Drag?.Invoke(this, eventData.position);
                    return;
            }
        }

        private void HandleStandaloneDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                if ((eventData.position - beginPosition).sqrMagnitude < dragThreshold * dragThreshold)
                {
                    return;
                }

                isDragging = true;
                BeginDrag?.Invoke(this, eventData.position);
            }

            Drag?.Invoke(this, eventData.position);
        }

        private void CompleteDrag(PointerEventData eventData)
        {
            if (endHandled)
            {
                return;
            }

            endHandled = true;
            isGestureArmed = false;
            var position = eventData != null ? eventData.position : beginPosition;

            if (gestureGate != null && gestureGate.CurrentMode == ScrollDragGestureGate.Mode.Scroll)
            {
                if (eventData != null)
                {
                    gestureGate.ForwardEndDrag(eventData);
                }

                gestureGate.ResetGesture();
                isDragging = false;
                activeEventData = null;
                return;
            }

            if (isDragging)
            {
                EndDrag?.Invoke(this, position);
            }

            if (gestureGate != null)
            {
                gestureGate.ResetGesture();
            }

            isDragging = false;
            activeEventData = null;
        }
    }
}
