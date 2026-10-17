using UnityEngine;

namespace Assets.Project.Code.Runtime.v2.PrefabPool
{
    public class PoolableObjectInfo : MonoBehaviour
    {
        public int PrefabKey { get; private set; }
        private bool Initialized { get; set; }

        public void Initialize(int prefabKey)
        {
            if (Initialized)
            {
                return;
            }

            Initialized = true;
            PrefabKey = prefabKey;
        }
    }
}