using System;
using Assets.Project.Code.Runtime.Configuration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using Zenject;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class GameMessageView : MonoBehaviour
    {
        [SerializeField] private LocalizeStringEvent localizeStringEvent;
        [SerializeField] private GameObject messageRoot;

        private GameMessagesConfig messagesConfig;
        private Tween hideTween;

        [Inject]
        private void Construct(GameMessagesConfig messagesConfig)
        {
            this.messagesConfig = messagesConfig;
        }

        public void Show(GameMessageId messageId)
        {
            if (messageId == GameMessageId.None)
            {
                return;
            }

            ShowInternal(Resolve(messageId));
        }

        private void ShowInternal(LocalizedString localizedString)
        {
            localizeStringEvent.StringReference = localizedString;
            localizeStringEvent.RefreshString();
            messageRoot.SetActive(true);

            hideTween?.Kill();
            hideTween = DOVirtual.DelayedCall(messagesConfig.VisibleDurationSeconds, Hide);
        }

        private void Hide()
        {
            messageRoot.SetActive(false);
            hideTween = null;
        }

        private void OnDisable()
        {
            hideTween?.Kill();
            hideTween = null;
        }

        private string ResolvePickSource(CubePickSource source)
        {
            switch (source)
            {
                case CubePickSource.Palette:
                    return messagesConfig.CubePickedFromPalette.GetLocalizedString();
                case CubePickSource.Tower:
                    return messagesConfig.CubePickedFromTower.GetLocalizedString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        private LocalizedString Resolve(GameMessageId messageId)
        {
            switch (messageId)
            {
                case GameMessageId.CubePlaced:
                    return messagesConfig.CubePlaced;
                case GameMessageId.CubePickedFromPalette:
                    return messagesConfig.CubePickedFromPalette;
                case GameMessageId.CubePickedFromTower:
                    return messagesConfig.CubePickedFromTower;
                case GameMessageId.CubeDestroyed:
                    return messagesConfig.CubeDestroyed;
                case GameMessageId.CubeAbsorbedByHole:
                    return messagesConfig.CubeAbsorbedByHole;
                case GameMessageId.CubeMissed:
                    return messagesConfig.CubeMissed;
                case GameMessageId.HeightLimitReached:
                    return messagesConfig.HeightLimitReached;
                case GameMessageId.ColorMismatch:
                    return messagesConfig.ColorMismatch;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageId), messageId, null);
            }
        }
    }
}
