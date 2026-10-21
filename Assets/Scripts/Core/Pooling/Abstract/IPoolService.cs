using UnityEngine;

namespace Core.Pooling.Abstract
{
    public interface IPoolService
    {
        void Prewarm<T>(T prefab, int count) where T : Component;

        T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component;

        void Release<T>(T instance) where T : Component;
    }
}
