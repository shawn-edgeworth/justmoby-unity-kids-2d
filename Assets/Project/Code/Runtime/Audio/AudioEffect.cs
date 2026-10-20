using UnityEngine;

namespace Assets.Project.Code.Runtime.Audio
{
    [CreateAssetMenu(fileName = nameof(AudioEffect), menuName = "Project/" + nameof(AudioEffect))]
    public sealed class AudioEffect : ScriptableObject
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] private bool loop;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.05f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.15f;

        public AudioClip Clip => clip;

        public float Volume => volume;

        public float Pitch => pitch;

        public bool Loop => loop;

        public float FadeInDuration => fadeInDuration;

        public float FadeOutDuration => fadeOutDuration;
    }
}
