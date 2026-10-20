using System;
using Assets.Project.Code.Runtime.Extensions;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class CubeView : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;
        [SerializeField] private Image overlayImage;
        [SerializeField] private DragListener dragListener;

        private Tween moveTween;
        private Action moveCompleteCallback;
        private bool isFollowing;
        private Vector2 followTarget;
        private float followLerpSpeed = 20f;

        public int Index { get; private set; }
        public string ColorId { get; private set; }
        public Color Color => image.color;
        public DragListener DragListener => dragListener;
        public Vector2 Position => rectTransform.position;

        public float Size => rectTransform.ToWorldRect().width;

        public void SetIndex(int index)
        {
            Index = index;
        }

        public void SetFollowLerpSpeed(float speed)
        {
            followLerpSpeed = speed;
        }

        public void ApplyVisual(string colorId, Color color)
        {
            ColorId = colorId;
            image.color = color;
        }

        public void SetMaterial(Material material)
        {
            image.material = material;
            overlayImage.material = material;
        }

        public void SetWorldSize(float worldSize)
        {
            rectTransform.localScale = Vector3.one;
            var parentScale = rectTransform.lossyScale.x;
            if (parentScale <= 0f)
            {
                throw new InvalidOperationException("Cube parent lossyScale.x must be positive.");
            }

            var sizeDelta = worldSize / parentScale;
            rectTransform.sizeDelta = new Vector2(sizeDelta, sizeDelta);
        }

        public void SetPosition(Vector2 position)
        {
            rectTransform.position = position;
        }

        public void BeginFollow(Vector2 position)
        {
            KillMoveTween();
            isFollowing = true;
            followTarget = position;
            SetPosition(position);
        }

        public void UpdateFollowTarget(Vector2 position)
        {
            followTarget = position;
        }

        public void EndFollow()
        {
            isFollowing = false;
        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            rectTransform.SetParent(parent, worldPositionStays);
            if (!worldPositionStays)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        public void MoveTo(Vector2 position, float duration, Ease ease, Action onComplete)
        {
            MoveTo(position, duration, ease, 0f, onComplete);
        }

        public void MoveTo(Vector2 position, float duration, Ease ease, float delay, Action onComplete)
        {
            EndFollow();
            KillMoveTween();
            moveCompleteCallback = onComplete;
            moveTween = rectTransform
                .DOMove(position, duration)
                .SetEase(ease)
                .SetDelay(delay)
                .OnComplete(OnMoveTweenComplete);
        }

        public void PlayRejectShake(
            float duration,
            float rotationStrength,
            float scaleStrength,
            int vibrato,
            Action onComplete)
        {
            EndFollow();
            KillMoveTween();
            moveCompleteCallback = onComplete;

            var sequence = DOTween.Sequence();
            sequence.Join(rectTransform.DOShakeRotation(
                duration,
                new Vector3(0f, 0f, rotationStrength),
                vibrato));
            sequence.Join(rectTransform.DOShakeScale(duration, scaleStrength, vibrato));
            sequence.OnComplete(OnRejectShakeComplete);
            moveTween = sequence;
        }

        public void PlayDestroy(float duration, Action onComplete)
        {
            EndFollow();
            KillMoveTween();
            moveCompleteCallback = onComplete;

            var sequence = DOTween.Sequence();
            sequence.Join(rectTransform.DOScale(0f, duration).SetEase(Ease.InBack));
            sequence.Join(image.DOFade(0f, duration));
            sequence.Join(overlayImage.DOFade(0f, duration));
            sequence.OnComplete(OnMoveTweenComplete);
            moveTween = sequence;
        }

        public void KillMoveTween()
        {
            if (moveTween == null)
            {
                return;
            }

            moveTween.Kill();
            moveTween = null;
            moveCompleteCallback = null;
        }

        public void OnSpawn()
        {
            ResetVisualState();
        }

        public void OnDespawn()
        {
            EndFollow();
            KillMoveTween();
            ColorId = null;
            image.material = null;
            overlayImage.material = null;
            ResetVisualState();
        }

        private void OnRejectShakeComplete()
        {
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            OnMoveTweenComplete();
        }

        private void OnMoveTweenComplete()
        {
            moveTween = null;
            var callback = moveCompleteCallback;
            moveCompleteCallback = null;
            if (callback != null)
            {
                callback.Invoke();
            }
        }

        private void LateUpdate()
        {
            if (!isFollowing)
            {
                return;
            }

            var t = 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime);
            rectTransform.position = Vector2.Lerp(rectTransform.position, followTarget, t);
        }

        private void ResetVisualState()
        {
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            var imageColor = image.color;
            imageColor.a = 1f;
            image.color = imageColor;

            var overlayColor = overlayImage.color;
            overlayColor.a = 1f;
            overlayImage.color = overlayColor;
        }
    }
}
