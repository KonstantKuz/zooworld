namespace Core.Actors.Abstract
{
    public interface IActor
    {
        int Id { get; }

        bool HasTrait<T>() where T : class;

        bool TryGetModule<T>(out T module) where T : class;
    }
}
