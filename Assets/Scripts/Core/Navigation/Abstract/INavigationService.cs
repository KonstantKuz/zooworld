using System.Collections.Generic;
using UnityEngine;

namespace Core.Navigation.Abstract
{
    public interface INavigationService
    {
        bool TryBuildPath(Vector3 from, Vector3 to, List<Vector3> corners);

        bool TryGetRandomPoint(out Vector3 point);

        void Rebuild();
    }
}
