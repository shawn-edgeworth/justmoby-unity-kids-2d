using System;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Audio
{
    public sealed class AudioEffectSource : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private AudioSource audioSource;

        private Tween playbackTween;
        private float fadeOutDuration;
        private Action finishedCallback;

        public void Play(AudioEffect effect, Action onFinished)
        {
            KillPlaybackTween();
            finishedCallback = onFinished;
            fadeOutDuration = effect.FadeOutDuration;

            audioSource.clip = effect.Clip;
            audioSource.pitch = effect.Pitch;
            audioSource.loop = effect.Loop;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = effect.FadeInDuration > 0f ? 0f : effect.Volume;
            audioSource.Play();

            if (effect.Loop)
            {
                if (effect.FadeInDuration > 0f)
                {
                    playbackTween = FadeVolume(effect.Volume, effect.FadeInDuration);
                }

                return;
            }

            playbackTween = BuildOneShotSequence(effect);
        }

        public void Stop(Action onStopped)
        {
            KillPlaybackTween();
            finishedCallback = null;

            if (fadeOutDuration <= 0f || !audioSource.isPlaying)
            {
                HardStop();
                if (onStopped != null)
                {
                    onStopped.Invoke();
                }

                return;
            }

            playbackTween = FadeVolume(0f, fadeOutDuration)
                .OnComplete(() =>
                {
                    playbackTween = null;
                    HardStop();
                    if (onStopped != null)
                    {
                        onStopped.Invoke();
                    }
                });
        }

        public void OnSpawn()
        {
        }

        public void OnDespawn()
        {
            KillPlaybackTween();
            finishedCallback = null;
            HardStop();
        }

        private Tween BuildOneShotSequence(AudioEffect effect)
        {
            var clipDuration = effect.Clip.length / Mathf.Max(0.01f, effect.Pitch);
            var fadeIn = effect.FadeInDuration;
            var fadeOut = effect.FadeOutDuration;
            var sequence = DOTween.Sequence();

            if (fadeIn > 0f)
            {
                sequence.Append(FadeVolume(effect.Volume, fadeIn));
            }
            else
            {
                audioSource.volume = effect.Volume;
            }

            var remaining = clipDuration - fadeIn;
            if (remaining < 0f)
            {
                remaining = 0f;
            }

            if (fadeOut > 0f && remaining > fadeOut)
            {
                sequence.AppendInterval(remaining - fadeOut);
                sequence.Append(FadeVolume(0f, fadeOut));
            }
            else if (fadeOut > 0f && remaining > 0f)
            {
                sequence.Append(FadeVolume(0f, remaining));
            }
            else if (remaining > 0f)
            {
                sequence.AppendInterval(remaining);
            }

            sequence.OnComplete(NotifyFinished);
            return sequence;
        }

        private Tweener FadeVolume(float endValue, float duration)
        {
            return audioSource.DOFade(endValue, duration).SetEase(Ease.Linear);
        }

        private void NotifyFinished()
        {
            playbackTween = null;
            HardStop();
            var callback = finishedCallback;
            finishedCallback = null;
            if (callback != null)
            {
                callback.Invoke();
            }
        }

        private void HardStop()
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.volume = 0f;
        }

        private void KillPlaybackTween()
        {
            if (playbackTween == null)
            {
                return;
            }

            playbackTween.Kill();
            playbackTween = null;
        }
    }
}
