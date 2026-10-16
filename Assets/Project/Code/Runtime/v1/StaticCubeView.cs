using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Project.Code.Runtime.v1
{
    public class StaticCubeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private GhostCubeView ghostCubeViewPrefab;
        [SerializeField] private DynamicCubeView dynamicCubeViewPrefab;
        [SerializeField] private RectTransform dragArea;
        [SerializeField] private CubeStackView cubeStackView;
        [SerializeField] private Image image;

        private GhostCubeView ghostCubeViewInstance;

        public void OnBeginDrag(PointerEventData eventData)
        {
            ghostCubeViewInstance = Instantiate(ghostCubeViewPrefab,
                eventData.position, Quaternion.identity, dragArea);
            ghostCubeViewInstance.Initialize(image.color);
        }

        public void OnDrag(PointerEventData eventData)
        {
            ghostCubeViewInstance.SetPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (cubeStackView.IsInside(ghostCubeViewInstance.Position))
            {
                if (cubeStackView.CanAddCube(ghostCubeViewInstance.Size))
                {
                    var dynamicCubeViewInstance = Instantiate(dynamicCubeViewPrefab,
                    eventData.position, Quaternion.identity, dragArea);
                    dynamicCubeViewInstance.Initialize(image.color);
                    cubeStackView.TryAddCube(dynamicCubeViewInstance);
                }
            }

            Destroy(ghostCubeViewInstance.gameObject);
            ghostCubeViewInstance = null;
        }
    }
}