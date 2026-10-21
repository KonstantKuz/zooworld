using System.Collections.Generic;
using System.Linq;
using System;
using Core.Actors.Abstract;
using Core.Actors.Configs;
using Core.Actors.Interactions.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Movement.Abstract;
using Core.Actors;
using Game.Actors.Traits.Abstract;
using R3;
using UnityEngine;

namespace Tests
{
    public sealed class PreyMarker : IPrey
    {
    }

    public sealed class PredatorMarker : IPredator
    {
    }

    public sealed class FakeActor : IActor
    {
        private readonly object[] traits;
        private readonly List<IActorModule> modules = new();

        public FakeActor(int id, params object[] traits)
        {
            Id = id;
            this.traits = traits;
        }

        public int Id { get; }

        public FakeActor With(IActorModule module)
        {
            modules.Add(module);
            return this;
        }

        public bool HasTrait<T>() where T : class
        {
            return traits.Any(trait => trait is T);
        }

        public bool TryGetModule<T>(out T module) where T : class
        {
            module = modules.OfType<T>().FirstOrDefault();
            return module != null;
        }

        public static FakeActor Prey(int id)
        {
            return new FakeActor(id, new PreyMarker());
        }

        public static FakeActor Predator(int id)
        {
            return new FakeActor(id, new PredatorMarker());
        }
    }

    public sealed class FakeHealth : IHealth
    {
        public bool IsAlive { get; private set; } = true;

        public void Die()
        {
            IsAlive = false;
        }
    }

    public sealed class FakeBody : IMovement, IPushable
    {
        public FakeBody(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
        public Vector3 LastPush { get; private set; }

        public void Move(MovementConfig gait, IRoute route, float deltaTime)
        {
        }

        public void Stop(float deltaTime)
        {
        }

        public void Push(Vector3 velocityChange)
        {
            LastPush = velocityChange;
        }
    }

    public sealed class FakeLifecycle : IActorLifecycle
    {
        public List<(IActor Victim, IActor Killer)> Kills { get; } = new();

        public int ActiveCount => 0;

        public IActor Spawn(ActorConfig config, Vector3 position)
        {
            throw new NotSupportedException();
        }

        public void Kill(IActor victim, IActor killer)
        {
            Kills.Add((victim, killer));
        }
    }

    public sealed class FakeFactory : IActorFactory
    {
        public List<IActor> Destroyed { get; } = new();

        public int ActiveCount => 0;

        public IActor Create(ActorConfig config, Vector3 position)
        {
            throw new NotSupportedException();
        }

        public void Destroy(IActor actor)
        {
            Destroyed.Add(actor);
        }
    }

    public sealed class FakeEvents : IActorEvents
    {
        public Subject<ActorDeath> Subject { get; } = new();

        public Observable<ActorDeath> Died => Subject;
    }

    public sealed class RecordingRule : IInteractionRule
    {
        private readonly bool handles;

        public RecordingRule(bool handles)
        {
            this.handles = handles;
        }

        public int Calls { get; private set; }

        public bool TryApply(IActor first, IActor second)
        {
            Calls++;
            return handles;
        }
    }
}
