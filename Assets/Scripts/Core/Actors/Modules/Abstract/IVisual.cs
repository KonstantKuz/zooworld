using UnityEngine;

namespace Core.Actors.Modules.Abstract
{
    public interface IVisual : IActorModule
    {
        void ShowTint(Color tint);

        void ClearTint();
    }
}
