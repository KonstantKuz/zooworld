using Core.Actors.Abstract;
using Core.Actors.Interactions.Abstract;
using Core.Actors.Modules.Abstract;

namespace Game.Actors.Modules.Collision
{
    public sealed class CollisionModule : ICollisionModule
    {
        private readonly ICollisionService collisions;

        public CollisionModule(ICollisionService collisions)
        {
            this.collisions = collisions;
        }

        public void OnCollided(IActor self, IActor other)
        {
            collisions.Report(self, other);
        }
    }
}
