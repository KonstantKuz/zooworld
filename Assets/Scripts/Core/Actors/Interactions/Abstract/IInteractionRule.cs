using Core.Actors.Abstract;

namespace Core.Actors.Interactions.Abstract
{
    public interface IInteractionRule
    {
        bool TryApply(IActor first, IActor second);
    }
}
