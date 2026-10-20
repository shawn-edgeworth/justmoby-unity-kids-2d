using System;
using System.Collections.Generic;
using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Infrastructure.Pooling;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    public sealed class PaletteView
    {
        private readonly IGameConfigProvider configProvider;
        private readonly CubePrefabsConfig cubePrefabsConfig;
        private readonly SceneRefs sceneRefs;
        private readonly IPrefabPool prefabPool;
        private readonly ScrollDragGestureGate scrollDragGestureGate;
        private readonly List<CubeView> views = new();

        public event Action<CubeView, Vector2> CubeBeginDrag;
        public event Action<CubeView, Vector2> CubeDrag;
        public event Action<CubeView, Vector2> CubeEndDrag;

        public PaletteView(
            IGameConfigProvider configProvider,
            CubePrefabsConfig cubePrefabsConfig,
            SceneRefs sceneRefs,
            IPrefabPool prefabPool,
            ScrollDragGestureGate scrollDragGestureGate)
        {
            this.configProvider = configProvider;
            this.cubePrefabsConfig = cubePrefabsConfig;
            this.sceneRefs = sceneRefs;
            this.prefabPool = prefabPool;
            this.scrollDragGestureGate = scrollDragGestureGate;
        }

        public void Build()
        {
            Clear();

            var palette = configProvider.GetPaletteCubes();
            for (var i = 0; i < palette.Count; i++)
            {
                var entry = palette[i];
                var view = prefabPool.Spawn(cubePrefabsConfig.CubePrefab.gameObject).GetComponent<CubeView>();
                view.SetParent(sceneRefs.PaletteRoot, false);
                view.ApplyVisual(entry.ColorId, entry.Color);
                view.SetIndex(i);
                view.DragListener.SetGestureGate(scrollDragGestureGate);
                view.DragListener.BeginDrag += OnBeginDrag;
                view.DragListener.Drag += OnDrag;
                view.DragListener.EndDrag += OnEndDrag;
                views.Add(view);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < views.Count; i++)
            {
                var view = views[i];
                if (view == null)
                {
                    continue;
                }

                view.DragListener.BeginDrag -= OnBeginDrag;
                view.DragListener.Drag -= OnDrag;
                view.DragListener.EndDrag -= OnEndDrag;
                view.KillMoveTween();
                prefabPool.Despawn(view.gameObject);
            }

            views.Clear();
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
