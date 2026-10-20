using Assets.Project.Code.Runtime.Configuration;
using Assets.Project.Code.Runtime.Model;

namespace Assets.Project.Code.Runtime.Gameplay
{
    public interface ICubePlacementRule
    {
        bool CanPlace(TowerModel tower, in PlacementContext context, out GameMessageId failMessage);
    }
}
