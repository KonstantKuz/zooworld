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
        private const int LevelAttempts = 32;

        private readonly Obstacle prefab;
        private readonly LevelSettings settings;
        private readonly IPoolService pools;
        private readonly INavigationService navigation;

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
            // A random layout can paint itself into a corner (an earlier wall blocks a later slot), so the whole
            // level is re-rolled instead of dropping a wall. The slots' home positions are the last resort.
            List<Placement> level = null;
            for (int attempt = 0; attempt < LevelAttempts && level == null; attempt++)
            {
                level = PlanLevel(jittered: true);
            }

            level ??= PlanLevel(jittered: false);

            for (int i = 0; i < level.Count; i++)
            {
                Spawn(level[i]);
            }

            navigation.Rebuild();
        }

        private List<Placement> PlanLevel(bool jittered)
        {
            List<Placement> level = new(settings.Slots.Count);
            for (int i = 0; i < settings.Slots.Count; i++)
            {
                ObstacleSlot slot = settings.Slots[i];
                if (jittered ? TryPlanJittered(slot, level) : TryPlan(slot, slot.Position, false, level))
                {
                    continue;
                }

                if (jittered)
                {
                    return null;
                }

                Debug.LogWarning($"Level slot {i} at {slot.Position} conflicts with the other slots, wall skipped.");
            }

            return level;
        }

        private bool TryPlanJittered(ObstacleSlot slot, List<Placement> level)
        {
            for (int attempt = 0; attempt < PlacementAttempts; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * settings.Jitter;
                bool turned = Random.value < 0.5f;
                if (TryPlan(slot, slot.Position + offset, turned, level))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryPlan(ObstacleSlot slot, Vector2 position, bool turned, List<Placement> level)
        {
            Vector2 footprint = turned ? new Vector2(slot.Size.y, slot.Size.x) : slot.Size;
            Vector3 center = new Vector3(position.x, settings.Height * 0.5f, position.y);
            Bounds bounds = new Bounds(center, new Vector3(footprint.x, settings.Height, footprint.y));

            Bounds padded = bounds;
            padded.Expand(new Vector3(settings.Clearance, 0f, settings.Clearance));
            for (int i = 0; i < level.Count; i++)
            {
                if (padded.Intersects(level[i].Bounds))
                {
                    return false;
                }
            }

            level.Add(new Placement(slot, bounds, turned));
            return true;
        }

        private void Spawn(Placement placement)
        {
            Obstacle obstacle = pools.Get(prefab, placement.Bounds.center, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(placement.Slot.Size.x, settings.Height, placement.Slot.Size.y);
            obstacle.transform.rotation = Quaternion.Euler(0f, placement.Turned ? 90f : 0f, 0f);
        }

        private readonly struct Placement
        {
            public ObstacleSlot Slot { get; }
            public Bounds Bounds { get; }
            public bool Turned { get; }

            public Placement(ObstacleSlot slot, Bounds bounds, bool turned)
            {
                Slot = slot;
                Bounds = bounds;
                Turned = turned;
            }
        }
    }
}
