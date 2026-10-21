using System.Collections.Generic;
using Core.Actors.Abstract;
using Core.Actors.Interactions.Abstract;
using Core.Actors.Modules.Abstract;
using Zenject;

namespace Core.Actors.Interactions
{
    public sealed class CollisionService : ICollisionService, IFixedTickable
    {
        private readonly List<IInteractionRule> rules;
        private readonly SortedDictionary<long, (IActor First, IActor Second)> contacts = new();

        public CollisionService(List<IInteractionRule> rules)
        {
            this.rules = rules;
        }

        public void Report(IActor self, IActor other)
        {
            bool selfFirst = self.Id < other.Id;
            IActor first = selfFirst ? self : other;
            IActor second = selfFirst ? other : self;
            contacts.TryAdd(((long)first.Id << 32) | (uint)second.Id, (first, second));
        }

        public void FixedTick()
        {
            foreach ((IActor first, IActor second) in contacts.Values)
            {
                Resolve(first, second);
            }

            contacts.Clear();
        }

        private void Resolve(IActor first, IActor second)
        {
            if (!first.IsAlive() || !second.IsAlive())
            {
                return;
            }

            foreach (IInteractionRule rule in rules)
            {
                if (rule.TryApply(first, second))
                {
                    return;
                }
            }
        }
    }
}
