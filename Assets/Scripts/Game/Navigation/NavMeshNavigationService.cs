using System.Collections.Generic;
using System;
using Core.Navigation.Abstract;
using Game.Navigation.Configs;
using UnityEngine.AI;
using UnityEngine;

namespace Game.Navigation
{
    public sealed class NavMeshNavigationService : INavigationService, IDisposable
    {
        private const float SampleDistance = 3f;
        private const float PlayfieldInset = 1.5f;
        private const int RandomPointAttempts = 8;
        private const float AreaHeight = 10f;

        private readonly IPlayfieldService playfield;
        private readonly NavigationSettings settings;
        private readonly NavMeshPath path = new();
        private readonly Vector3[] cornerBuffer = new Vector3[64];
        private NavMeshDataInstance instance;

        public NavMeshNavigationService(IPlayfieldService playfield, NavigationSettings settings)
        {
            this.playfield = playfield;
            this.settings = settings;
        }

        public void Rebuild()
        {
            Physics.SyncTransforms();
            Bounds area = new Bounds(Vector3.zero, new Vector3(settings.AreaSize, AreaHeight, settings.AreaSize));
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(area, ~0, NavMeshCollectGeometry.PhysicsColliders, 0,
                new List<NavMeshBuildMarkup>(), sources);

            NavMeshBuildSettings agent = NavMesh.GetSettingsByIndex(0);
            agent.agentRadius = settings.AgentRadius;
            NavMeshData data = NavMeshBuilder.BuildNavMeshData(agent, sources, area, Vector3.zero, Quaternion.identity);

            Dispose();
            instance = NavMesh.AddNavMeshData(data);
        }

        public void Dispose()
        {
            if (instance.valid)
            {
                NavMesh.RemoveNavMeshData(instance);
            }
        }

        public bool TryBuildPath(Vector3 from, Vector3 to, List<Vector3> corners)
        {
            corners.Clear();
            if (!NavMesh.SamplePosition(from, out NavMeshHit start, SampleDistance, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(to, out NavMeshHit end, SampleDistance, NavMesh.AllAreas) ||
                !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) ||
                path.status == NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            int count = path.GetCornersNonAlloc(cornerBuffer);
            for (int i = 0; i < count; i++)
            {
                corners.Add(cornerBuffer[i]);
            }

            return count > 0;
        }

        public bool TryGetRandomPoint(out Vector3 point)
        {
            for (int i = 0; i < RandomPointAttempts; i++)
            {
                if (NavMesh.SamplePosition(playfield.RandomPoint(PlayfieldInset), out NavMeshHit hit, SampleDistance,
                        NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = default;
            return false;
        }
    }
}
