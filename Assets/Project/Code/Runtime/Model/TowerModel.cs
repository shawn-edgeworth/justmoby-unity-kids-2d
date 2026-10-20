using System.Collections.Generic;

namespace Assets.Project.Code.Runtime.Model
{
    public sealed class TowerModel
    {
        private readonly List<CubeData> cubes = new();

        public IReadOnlyList<CubeData> Cubes => cubes;

        public int Count => cubes.Count;

        public bool IsEmpty => cubes.Count == 0;

        public void Add(CubeData cube)
        {
            cubes.Add(cube);
        }

        public CubeData RemoveAt(int index)
        {
            var cube = cubes[index];
            cubes.RemoveAt(index);
            return cube;
        }

        public CubeData GetAt(int index)
        {
            return cubes[index];
        }

        public bool TryGetTop(out CubeData top)
        {
            if (cubes.Count == 0)
            {
                top = null;
                return false;
            }

            top = cubes[^1];
            return true;
        }

        public void ReplaceAll(IReadOnlyList<CubeData> source)
        {
            cubes.Clear();
            for (var i = 0; i < source.Count; i++)
            {
                cubes.Add(source[i]);
            }
        }

        public void Clear()
        {
            cubes.Clear();
        }
    }
}
