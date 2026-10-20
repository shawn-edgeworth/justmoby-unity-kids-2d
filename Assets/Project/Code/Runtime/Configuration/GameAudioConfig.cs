using Assets.Project.Code.Runtime.Audio;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Configuration
{
    [CreateAssetMenu(fileName = nameof(GameAudioConfig), menuName = "Project/" + nameof(GameAudioConfig))]
    public sealed class GameAudioConfig : ScriptableObject
    {
        [field: SerializeField] public AudioEffectSource SourcePrefab { get; private set; }

        [field: SerializeField] public AudioEffect Pick { get; private set; }
        [field: SerializeField] public AudioEffect PlaceOrSettle { get; private set; }
        [field: SerializeField] public AudioEffect InvalidPlace { get; private set; }
        [field: SerializeField] public AudioEffect DestroyWhoosh { get; private set; }
        [field: SerializeField] public AudioEffect HoleHoverLoop { get; private set; }
        [field: SerializeField] public AudioEffect HoleFlight { get; private set; }

        [field: SerializeField, Range(0f, 1f)]
        public float ConsumeVfxProgress { get; private set; } = 0.4f;

        [field: SerializeField, Min(0f)]
        public float OneShotThrottleSeconds { get; private set; } = 0.1f;
    }
}
