using Core.Actors.Abstract;

namespace Core.Actors.Interactions.Abstract
{
    public abstract class InteractionRule<TFirst, TSecond> : IInteractionRule
        where TFirst : class
        where TSecond : class
    {
        public bool TryApply(IActor first, IActor second)
        {
            if (Matches(first, second))
            {
                Apply(first, second);
                return true;
            }

            if (Matches(second, first))
            {
                Apply(second, first);
                return true;
            }

            return false;
        }

        protected abstract void Apply(IActor first, IActor second);

        private static bool Matches(IActor first, IActor second)
        {
            return first.HasTrait<TFirst>() && second.HasTrait<TSecond>();
        }
    }
}
