using UnityEngine;

namespace Assets.Project.Code.Runtime.Infrastructure.Pooling
{
    public sealed class PoolableObjectInfo : MonoBehaviour
    {
        public int PrefabKey { get; private set; }

        public void Initialize(int prefabKey)
        {
            PrefabKey = prefabKey;
        }
    }
}
