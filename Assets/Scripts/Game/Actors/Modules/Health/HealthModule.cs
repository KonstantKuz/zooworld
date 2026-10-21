using Core.Actors.Modules.Abstract;
using R3;

namespace Game.Actors.Modules.Health
{
    public sealed class HealthModule : IHealth
    {
        private readonly CompositeDisposable lifetime;

        public HealthModule(CompositeDisposable lifetime)
        {
            this.lifetime = lifetime;
        }

        public bool IsAlive => !lifetime.IsDisposed;

        public void Die()
        {
            lifetime.Dispose();
        }
    }
}
