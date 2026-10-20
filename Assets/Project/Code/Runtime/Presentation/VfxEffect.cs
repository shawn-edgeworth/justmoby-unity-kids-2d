using System;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class VfxEffect : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private UIParticle particle;
        [SerializeField] private float lifetime = 2f;

        private Tween finishTween;
        private Action finishedCallback;

        public void Play(Vector3 worldPosition, Action onFinished)
        {
            KillFinishTween();
            finishedCallback = onFinished;
            transform.position = worldPosition;
            particle.Stop();
            particle.Play();
            finishTween = DOVirtual.DelayedCall(lifetime, NotifyFinished);
        }

        public void OnSpawn()
        {
        }

        public void OnDespawn()
        {
            KillFinishTween();
            finishedCallback = null;
            particle.Stop();
        }

        private void NotifyFinished()
        {
            finishTween = null;
            var callback = finishedCallback;
            finishedCallback = null;
            if (callback != null)
            {
                callback.Invoke();
            }
        }

        private void KillFinishTween()
        {
            if (finishTween == null)
            {
                return;
            }

            finishTween.Kill();
            finishTween = null;
        }
    }
}
