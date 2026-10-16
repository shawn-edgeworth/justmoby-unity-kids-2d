using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Project.Code.Runtime.v1
{
    public class DynamicCubeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        public Vector2 Position => rectTransform.position;
        public float Width => rectTransform.rect.width;
        public float Height => rectTransform.rect.height;

        public float Top => rectTransform.TransformVector(Vector3.up * rectTransform.rect.yMax).y;

        public event Action<DynamicCubeView> OnDragStarted;

        public void Initialize(Color color)
        {
            image.color = color;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDragStarted?.Invoke(this);
            SetInStack(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var elipseArea = FindObjectOfType<EllipseArea>();

            if (elipseArea != null && elipseArea.ContainsPoint(eventData.position))
            {
                Debug.Log("Cube inisde hole!");
            }

            Destroy(gameObject);
        }

        public void UpdatePosition(Vector2 position)
        {
            rectTransform.position = position;
        }

        public void SetInStack(bool inStack)
        {
            rectTransform.pivot = inStack ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        }
    }
}