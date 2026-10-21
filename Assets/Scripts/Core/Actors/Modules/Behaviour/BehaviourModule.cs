using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Abstract;

namespace Core.Actors.Modules.Behaviour
{
    public sealed class BehaviourModule : ITickModule, IInitializableModule, IGizmoModule
    {
        private readonly IBehaviourNode root;

        public BehaviourModule(IBehaviourNode root)
        {
            this.root = root;
        }

        public void Initialize()
        {
            root.Initialize();
        }

        public void Tick(float deltaTime)
        {
            root.Tick(deltaTime);
        }

        public void DrawGizmos()
        {
            root.DrawGizmos();
        }
    }
}
