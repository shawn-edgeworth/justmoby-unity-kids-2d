using System;
using Assets.Project.Code.Runtime.Model;
using Assets.Project.Code.Runtime.Persistence;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public sealed class GamePersistence
    {
        private readonly TowerModel tower;
        private readonly ISaveService saveService;
        private readonly GameSaveMapper saveMapper;

        public GamePersistence(
            TowerModel tower,
            ISaveService saveService,
            GameSaveMapper saveMapper)
        {
            this.tower = tower;
            this.saveService = saveService;
            this.saveMapper = saveMapper;
        }

        public void LoadOrStartNew()
        {
            if (!saveService.TryLoad(out GameSaveData data))
            {
                tower.Clear();
                return;
            }

            try
            {
                var needsResave = saveMapper.ApplyToTower(data, tower);
                if (needsResave)
                {
                    Save();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                tower.Clear();
            }
        }

        public void Save()
        {
            saveService.Save(saveMapper.ToSaveData(tower));
        }
    }
}
