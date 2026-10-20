using System.Collections.Generic;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Audio
{
    public sealed class AudioService
    {
        private readonly IPrefabPool prefabPool;
        private readonly GameAudioConfig audioConfig;
        private readonly Dictionary<AudioEffect, float> lastOneShotPlayTimes = new();

        private AudioEffectSource holeHoverInstance;

        public AudioService(IPrefabPool prefabPool, GameAudioConfig audioConfig)
        {
            this.prefabPool = prefabPool;
            this.audioConfig = audioConfig;
        }

        public void PlayPick()
        {
            PlayOneShot(audioConfig.Pick);
        }

        public void PlayPlaceOrSettle()
        {
            PlayOneShot(audioConfig.PlaceOrSettle);
        }

        public void PlayInvalidPlace()
        {
            PlayOneShot(audioConfig.InvalidPlace);
        }

        public void PlayDestroyWhoosh()
        {
            PlayOneShot(audioConfig.DestroyWhoosh);
        }

        public void PlayHoleFlight()
        {
            PlayOneShot(audioConfig.HoleFlight);
        }

        public void StartHoleHoverLoop()
        {
            if (holeHoverInstance != null)
            {
                return;
            }

            holeHoverInstance = SpawnSource();
            holeHoverInstance.Play(audioConfig.HoleHoverLoop, null);
        }

        public void StopHoleHoverLoop()
        {
            if (holeHoverInstance == null)
            {
                return;
            }

            var instance = holeHoverInstance;
            holeHoverInstance = null;
            instance.Stop(() => prefabPool.Despawn(instance.gameObject));
        }

        private void PlayOneShot(AudioEffect effect)
        {
            if (IsOneShotThrottled(effect))
            {
                return;
            }

            lastOneShotPlayTimes[effect] = Time.unscaledTime;
            var source = SpawnSource();
            source.Play(effect, () => prefabPool.Despawn(source.gameObject));
        }

        private bool IsOneShotThrottled(AudioEffect effect)
        {
            var throttleSeconds = audioConfig.OneShotThrottleSeconds;
            if (throttleSeconds <= 0f)
            {
                return false;
            }

            if (!lastOneShotPlayTimes.TryGetValue(effect, out var lastPlayTime))
            {
                return false;
            }

            return Time.unscaledTime - lastPlayTime < throttleSeconds;
        }

        private AudioEffectSource SpawnSource()
        {
            return prefabPool.Spawn(audioConfig.SourcePrefab.gameObject).GetComponent<AudioEffectSource>();
        }
    }
}
