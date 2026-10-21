namespace Core.Actors.Modules.Movement.Abstract
{
    public interface IMovementStrategy
    {
        void Tick(IRoute route, float deltaTime);

        void Stop(float deltaTime);
    }
}
