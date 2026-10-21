using System;
using System.Collections.Generic;
using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour;
using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Movement.Abstract;
using Core.Navigation.Abstract;
using Game.Actors.Status;
using Game.Actors.Status.Abstract;
using R3;
using UnityEngine;

namespace Tests
{
    /// <summary>Behaviour-tree leaf that answers with a scripted sequence of statuses and counts its calls.</summary>
    public sealed class ScriptedNode : IBehaviourNode
    {
        private readonly Queue<BehaviourStatus> script;
        private readonly BehaviourStatus fallback;

        public ScriptedNode(BehaviourStatus fallback, params BehaviourStatus[] script)
        {
            this.fallback = fallback;
            this.script = new Queue<BehaviourStatus>(script);
        }

        public int Ticks { get; private set; }
        public int Resets { get; private set; }
        public int Initializations { get; private set; }

        public void Initialize()
        {
            Initializations++;
        }

        public BehaviourStatus Tick(float deltaTime)
        {
            Ticks++;
            return script.Count > 0 ? script.Dequeue() : fallback;
        }

        public void Reset()
        {
            Resets++;
        }
    }

    public sealed class RecordingMovement : IMovement
    {
        public Vector3 Position { get; set; }
        public List<IRoute> Routes { get; } = new();
        public int Stops { get; private set; }

        public void Move(MovementConfig gait, IRoute route, float deltaTime)
        {
            Routes.Add(route);
        }

        public void Stop(float deltaTime)
        {
            Stops++;
        }
    }

    public sealed class FakeVisual : IVisual
    {
        public Color? Tint { get; private set; }

        public void ShowTint(Color tint)
        {
            Tint = tint;
        }

        public void ClearTint()
        {
            Tint = null;
        }
    }

    public sealed class FakeStatuses : IActorStatus
    {
        private readonly HashSet<(int, Type)> active = new();

        public Observable<ActorStatusChanged> Changed => Observable.Empty<ActorStatusChanged>();

        public bool Has<T>(IActor actor) where T : ActorStatus
        {
            return active.Contains((actor.Id, typeof(T)));
        }

        public void Set<T>(IActor actor, bool isActive) where T : ActorStatus, new()
        {
            if (isActive)
            {
                active.Add((actor.Id, typeof(T)));
            }
            else
            {
                active.Remove((actor.Id, typeof(T)));
            }
        }
    }

    /// <summary>Navigation on an open field: every path is a straight line, unless blocked.</summary>
    public sealed class FakeNavigation : INavigationService
    {
        public bool Blocked { get; set; }
        public int Rebuilds { get; private set; }
        public Vector3 RandomPoint { get; set; } = new(10f, 0f, 0f);

        public bool TryBuildPath(Vector3 from, Vector3 to, List<Vector3> corners)
        {
            corners.Clear();
            if (Blocked)
            {
                return false;
            }

            corners.Add(from);
            corners.Add(to);
            return true;
        }

        public bool TryGetRandomPoint(out Vector3 point)
        {
            point = RandomPoint;
            return !Blocked;
        }

        public void Rebuild()
        {
            Rebuilds++;
        }
    }

    /// <summary>Playfield that hands out the given points in turn.</summary>
    public sealed class FakePlayfield : IPlayfieldService
    {
        private readonly Vector3[] points;
        private int next;

        public FakePlayfield(params Vector3[] points)
        {
            this.points = points.Length > 0 ? points : new[] { Vector3.zero };
        }

        public bool Inside { get; set; } = true;

        public bool Contains(Vector3 position)
        {
            return Inside;
        }

        public Vector3 RandomPoint(float inset)
        {
            return points[next++ % points.Length];
        }
    }

    public sealed class FakeRoute : IRoute
    {
        public Vector3 NextDirection(Vector3 position, float deltaTime)
        {
            return Vector3.zero;
        }
    }
}
