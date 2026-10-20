using UnityEngine;
using UnityEngine.Localization;

namespace Assets.Project.Code.Runtime.Configuration
{
    [CreateAssetMenu(fileName = nameof(GameMessagesConfig), menuName = "Project/" + nameof(GameMessagesConfig))]
    public sealed class GameMessagesConfig : ScriptableObject
    {
        [SerializeField] private float visibleDurationSeconds = 2f;
        [SerializeField] private LocalizedString cubePickedFromPalette;
        [SerializeField] private LocalizedString cubePickedFromTower;
        [SerializeField] private LocalizedString cubePlaced;
        [SerializeField] private LocalizedString cubeDestroyed;
        [SerializeField] private LocalizedString cubeAbsorbedByHole;
        [SerializeField] private LocalizedString cubeMissed;
        [SerializeField] private LocalizedString heightLimitReached;
        [SerializeField] private LocalizedString colorMismatch;

        public float VisibleDurationSeconds => visibleDurationSeconds;

        public LocalizedString CubePickedFromPalette => cubePickedFromPalette;

        public LocalizedString CubePickedFromTower => cubePickedFromTower;

        public LocalizedString CubePlaced => cubePlaced;

        public LocalizedString CubeDestroyed => cubeDestroyed;

        public LocalizedString CubeAbsorbedByHole => cubeAbsorbedByHole;

        public LocalizedString CubeMissed => cubeMissed;

        public LocalizedString HeightLimitReached => heightLimitReached;

        public LocalizedString ColorMismatch => colorMismatch;
    }
}
