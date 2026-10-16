using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Code.Runtime.v1
{
    public class GhostCubeView : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        public Vector2 Position => rectTransform.position;
        public Vector2 Size => rectTransform.rect.size;
        public float Top => rectTransform.TransformVector(Vector3.up * rectTransform.rect.yMax).y;

        public void Initialize(Color color)
        {
            image.color = color;
        }

        public void SetPosition(Vector2 position) => rectTransform.position = position;
    }
}