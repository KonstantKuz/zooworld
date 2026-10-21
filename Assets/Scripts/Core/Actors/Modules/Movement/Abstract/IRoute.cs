using UnityEngine;

namespace Core.Actors.Modules.Movement.Abstract
{
    public interface IRoute
    {
        Vector3 NextDirection(Vector3 position, float deltaTime);
    }
}
