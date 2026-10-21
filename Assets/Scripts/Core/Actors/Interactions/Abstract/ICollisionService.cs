using Core.Actors.Abstract;

namespace Core.Actors.Interactions.Abstract
{
    public interface ICollisionService
    {
        void Report(IActor self, IActor other);
    }
}
