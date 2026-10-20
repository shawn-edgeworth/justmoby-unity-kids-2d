using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class SceneRefs : MonoBehaviour
    {
        [field: SerializeField] public Transform PaletteRoot { get; private set; }
        [field: SerializeField] public Transform DragLayer { get; private set; }
        [field: SerializeField] public Transform TowerRoot { get; private set; }
        [field: SerializeField] public TowerAreaView TowerArea { get; private set; }
        [field: SerializeField] public HoleView Hole { get; private set; }
        [field: SerializeField] public Transform HoleClipRoot { get; private set; }
        [field: SerializeField] public RectTransform HoleAbovePoint { get; private set; }
        [field: SerializeField] public RectTransform HoleInsidePoint { get; private set; }
        [field: SerializeField] public GameMessageView MessageView { get; private set; }
        [field: SerializeField] public ScrollRect PaletteScrollRect { get; private set; }
        [field: SerializeField] public ScrollDragGestureGate PaletteScrollGestureGate { get; private set; }
        [field: SerializeField] public Material HoleAbsorbMaterial { get; private set; }
    }
}
