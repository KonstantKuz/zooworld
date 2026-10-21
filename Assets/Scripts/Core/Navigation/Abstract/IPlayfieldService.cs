using UnityEngine;

namespace Core.Navigation.Abstract
{
    public interface IPlayfieldService
    {
        bool Contains(Vector3 position);
        Vector3 RandomPoint(float inset);
    }
}
