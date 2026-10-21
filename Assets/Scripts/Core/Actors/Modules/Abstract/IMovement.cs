using Core.Actors.Modules.Movement.Abstract;
using UnityEngine;

namespace Core.Actors.Modules.Abstract
{
    public interface IMovement : IActorModule
    {
        Vector3 Position { get; }

        void Move(MovementConfig gait, IRoute route, float deltaTime);

        void Stop(float deltaTime);
    }
}
