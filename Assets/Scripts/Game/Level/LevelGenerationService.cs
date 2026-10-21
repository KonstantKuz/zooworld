using System.Collections.Generic;
using Core.Navigation.Abstract;
using Core.Pooling.Abstract;
using Game.Level.Configs;
using UnityEngine;
using Zenject;

namespace Game.Level
{
    public sealed class LevelGenerationService : IInitializable
    {
        private const int PlacementAttempts = 12;

        private readonly Obstacle prefab;
        private readonly LevelSettings settings;
        private readonly IPoolService pools;
        private readonly INavigationService navigation;
        private readonly List<Bounds> placed = new();

        public LevelGenerationService(Obstacle prefab, LevelSettings settings, IPoolService pools,
            INavigationService navigation)
        {
            this.prefab = prefab;
            this.settings = settings;
            this.pools = pools;
            this.navigation = navigation;
        }

        public void Initialize()
        {
            for (int i = 0; i < settings.Slots.Count; i++)
            {
                PlaceWall(settings.Slots[i]);
            }

            navigation.Rebuild();
        }

        private void PlaceWall(ObstacleSlot slot)
        {
            for (int attempt = 0; attempt < PlacementAttempts; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * settings.Jitter;
                bool turned = Random.value < 0.5f;
                if (TryPlace(slot, slot.Position + offset, turned))
                {
                    return;
                }
            }

            TryPlace(slot, slot.Position, false);
        }

        private bool TryPlace(ObstacleSlot slot, Vector2 position, bool turned)
        {
            Vector2 footprint = turned ? new Vector2(slot.Size.y, slot.Size.x) : slot.Size;
            Vector3 center = new Vector3(position.x, settings.Height * 0.5f, position.y);
            Bounds bounds = new Bounds(center, new Vector3(footprint.x, settings.Height, footprint.y));

            Bounds padded = bounds;
            padded.Expand(new Vector3(settings.Clearance, 0f, settings.Clearance));
            for (int i = 0; i < placed.Count; i++)
            {
                if (padded.Intersects(placed[i]))
                {
                    return false;
                }
            }

            Obstacle obstacle = pools.Get(prefab, center, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(slot.Size.x, settings.Height, slot.Size.y);
            obstacle.transform.rotation = Quaternion.Euler(0f, turned ? 90f : 0f, 0f);
            placed.Add(bounds);
            return true;
        }
    }
}
