using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    [CreateAssetMenu(fileName = nameof(GameAnimationConfig), menuName = "Project/" + nameof(GameAnimationConfig))]
    public sealed class GameAnimationConfig : ScriptableObject
    {
        [field: SerializeField] public float HoleAbsorbDuration { get; private set; } = 1f;
        [field: SerializeField] public float DestroyDuration { get; private set; } = 0.35f;
        [field: SerializeField] public float SettleDuration { get; private set; } = 0.25f;
        [field: SerializeField] public float SettleStagger { get; private set; } = 0.06f;
        [field: SerializeField] public float SettlePositionEpsilon { get; private set; } = 0.01f;
        [field: SerializeField] public float FollowLerpSpeed { get; private set; } = 20f;
    }
}
