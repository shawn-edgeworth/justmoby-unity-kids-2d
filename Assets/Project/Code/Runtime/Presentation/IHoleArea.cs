using UnityEngine;

namespace Assets.Project.Code.Runtime.Presentation
{
    public interface IHoleArea
    {
        bool Contains(Vector2 worldPoint);
    }
}
