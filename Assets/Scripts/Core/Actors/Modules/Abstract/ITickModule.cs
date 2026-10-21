namespace Core.Actors.Modules.Abstract
{
    public interface ITickModule : IActorModule
    {
        void Tick(float deltaTime);
    }
}
