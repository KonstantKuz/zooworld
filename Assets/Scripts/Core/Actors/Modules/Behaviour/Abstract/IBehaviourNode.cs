namespace Core.Actors.Modules.Behaviour.Abstract
{
    public interface IBehaviourNode
    {
        void Initialize();

        BehaviourStatus Tick(float deltaTime);

        void DrawGizmos()
        {
        }

        void Reset();
    }
}
