using System;
using System.Collections.Generic;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using Assets.Project.Code.Runtime.Model;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class TowerView
    {
        private readonly TowerModel tower;
        private readonly TowerLayout layout;
        private readonly IGameConfigProvider configProvider;
        private readonly GameAnimationConfig animationConfig;
        private readonly ITowerArea towerArea;
        private readonly CubePrefabsConfig cubePrefabsConfig;
        private readonly SceneRefs sceneRefs;
        private readonly IPrefabPool prefabPool;
        private readonly List<CubeView> views = new();
        private readonly List<Vector2> positionBuffer = new();

        private int settleRemaining;
        private Action settleCompleteCallback;

        public event Action<CubeView, Vector2> CubeBeginDrag;
        public event Action<CubeView, Vector2> CubeDrag;
        public event Action<CubeView, Vector2> CubeEndDrag;
        public event Action CubeSettled;

        public TowerView(
            TowerModel tower,
            TowerLayout layout,
            IGameConfigProvider configProvider,
            GameAnimationConfig animationConfig,
            ITowerArea towerArea,
            CubePrefabsConfig cubePrefabsConfig,
            SceneRefs sceneRefs,
            IPrefabPool prefabPool)
        {
            this.tower = tower;
            this.layout = layout;
            this.configProvider = configProvider;
            this.animationConfig = animationConfig;
            this.towerArea = towerArea;
            this.cubePrefabsConfig = cubePrefabsConfig;
            this.sceneRefs = sceneRefs;
            this.prefabPool = prefabPool;
        }

        public void Rebuild()
        {
            ClearViews();

            var cubes = tower.Cubes;
            layout.CalculatePositions(tower, towerArea, positionBuffer);

            for (var i = 0; i < cubes.Count; i++)
            {
                var view = CreateView(i, cubes[i]);
                view.SetPosition(positionBuffer[i]);
                views.Add(view);
            }
        }

        public CubeView Spawn(CubeData cube, Vector2 fromPosition, bool animate, Action onComplete)
        {
            var index = tower.Count - 1;
            layout.CalculatePositions(tower, towerArea, positionBuffer);
            var target = positionBuffer[index];

            var view = CreateView(index, cube);
            view.SetPosition(fromPosition);
            views.Add(view);

            if (animate)
            {
                view.MoveTo(target, animationConfig.SettleDuration, Ease.OutQuad, onComplete);
            }
            else
            {
                view.SetPosition(target);
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            }

            return view;
        }

        public CubeView DetachAt(int index)
        {
            var view = views[index];
            views.RemoveAt(index);
            RefreshIndices();
            view.SetParent(sceneRefs.DragLayer, true);
            return view;
        }

        public void ApplySettle(bool animate, Action onComplete)
        {
            layout.CalculatePositions(tower, towerArea, positionBuffer);

            if (!animate || views.Count == 0)
            {
                for (var i = 0; i < views.Count; i++)
                {
                    views[i].SetPosition(positionBuffer[i]);
                }

                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return;
            }

            var startIndex = -1;
            var epsilon = animationConfig.SettlePositionEpsilon;
            var epsilonSqr = epsilon * epsilon;
            for (var i = 0; i < views.Count; i++)
            {
                if ((views[i].Position - positionBuffer[i]).sqrMagnitude > epsilonSqr)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex < 0)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return;
            }

            settleCompleteCallback = onComplete;
            settleRemaining = views.Count - startIndex;
            for (var i = startIndex; i < views.Count; i++)
            {
                var delay = (i - startIndex) * animationConfig.SettleStagger;
                views[i].MoveTo(
                    positionBuffer[i],
                    animationConfig.SettleDuration,
                    Ease.OutQuad,
                    delay,
                    OnSettleCubeComplete);
            }
        }

        public void Despawn(CubeView view)
        {
            if (view == null)
            {
                return;
            }

            Unbind(view);
            view.KillMoveTween();
            prefabPool.Despawn(view.gameObject);
        }

        public void ClearViews()
        {
            for (var i = 0; i < views.Count; i++)
            {
                var view = views[i];
                if (view == null)
                {
                    continue;
                }

                Despawn(view);
            }

            views.Clear();
        }

        private void OnSettleCubeComplete()
        {
            CubeSettled?.Invoke();
            settleRemaining--;
            if (settleRemaining > 0)
            {
                return;
            }

            var callback = settleCompleteCallback;
            settleCompleteCallback = null;
            if (callback != null)
            {
                callback.Invoke();
            }
        }

        private CubeView CreateView(int index, CubeData cube)
        {
            if (!configProvider.TryGetColor(cube.ColorId, out var color))
            {
                throw new InvalidOperationException($"Unknown color id '{cube.ColorId}'.");
            }

            var view = prefabPool.Spawn(cubePrefabsConfig.TowerCubePrefab.gameObject).GetComponent<CubeView>();
            view.SetParent(sceneRefs.TowerRoot, false);
            view.ApplyVisual(cube.ColorId, color);
            view.SetIndex(index);
            view.SetWorldSize(cube.Size);
            view.SetFollowLerpSpeed(animationConfig.FollowLerpSpeed);
            Bind(view);
            return view;
        }

        private void RefreshIndices()
        {
            for (var i = 0; i < views.Count; i++)
            {
                views[i].SetIndex(i);
            }
        }

        private void Bind(CubeView view)
        {
            view.DragListener.BeginDrag += OnBeginDrag;
            view.DragListener.Drag += OnDrag;
            view.DragListener.EndDrag += OnEndDrag;
        }

        private void Unbind(CubeView view)
        {
            view.DragListener.BeginDrag -= OnBeginDrag;
            view.DragListener.Drag -= OnDrag;
            view.DragListener.EndDrag -= OnEndDrag;
        }

        private void OnBeginDrag(DragListener listener, Vector2 position)
        {
            var view = listener.GetComponent<CubeView>();
            CubeBeginDrag?.Invoke(view, position);
        }

        private void OnDrag(DragListener listener, Vector2 position)
        {
            var view = listener.GetComponent<CubeView>();
            CubeDrag?.Invoke(view, position);
        }

        private void OnEndDrag(DragListener listener, Vector2 position)
        {
            var view = listener.GetComponent<CubeView>();
            CubeEndDrag?.Invoke(view, position);
        }
    }
}
