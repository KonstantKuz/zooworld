using System.Collections.Generic;
using Core.Actors.Modules.Behaviour;
using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Behaviour.Nodes;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public sealed class BehaviourTreeTests
    {
        private const BehaviourStatus Running = BehaviourStatus.Running;
        private const BehaviourStatus Success = BehaviourStatus.Success;
        private const BehaviourStatus Failure = BehaviourStatus.Failure;

        private static List<IBehaviourNode> Children(params IBehaviourNode[] nodes)
        {
            return new List<IBehaviourNode>(nodes);
        }

        [Test]
        public void Wait_RunsUntilTheTimeIsUpThenSucceedsAndStartsOver()
        {
            WaitNode wait = CreateWait(1f, out _);

            Assert.AreEqual(Running, wait.Tick(0.4f));
            Assert.AreEqual(Running, wait.Tick(0.4f));
            Assert.AreEqual(Success, wait.Tick(0.4f));
            Assert.AreEqual(Running, wait.Tick(0.4f), "the timer must restart after succeeding");
        }

        [Test]
        public void Wait_BringsTheActorToAStopWhileItWaits()
        {
            WaitNode wait = CreateWait(1f, out RecordingMovement movement);

            wait.Tick(0.4f);
            wait.Tick(0.4f);

            Assert.AreEqual(2, movement.Stops);
        }

        [Test]
        public void Wait_WorksForAnActorThatCannotMove()
        {
            WaitNode wait = new(FakeActor.Prey(1), 1f);
            wait.Initialize();

            Assert.AreEqual(Running, wait.Tick(0.4f));
        }

        [Test]
        public void Wait_ResetDropsTheElapsedTime()
        {
            WaitNode wait = CreateWait(1f, out _);
            wait.Tick(0.9f);

            wait.Reset();

            Assert.AreEqual(Running, wait.Tick(0.9f));
        }

        [Test]
        public void Sequence_RunsChildrenInOrderAndSucceedsWhenAllDo()
        {
            ScriptedNode first = new(Success);
            ScriptedNode second = new(Success);
            SequenceNode sequence = new(Children(first, second));

            Assert.AreEqual(Success, sequence.Tick(0.1f));
            Assert.AreEqual(1, first.Ticks);
            Assert.AreEqual(1, second.Ticks);
        }

        [Test]
        public void Sequence_WaitsOnARunningChildWithoutRetickingTheFinishedOnes()
        {
            ScriptedNode first = new(Success);
            ScriptedNode second = new(Success, Running, Running, Running);
            SequenceNode sequence = new(Children(first, second));

            Assert.AreEqual(Running, sequence.Tick(0.1f));
            Assert.AreEqual(Running, sequence.Tick(0.1f));
            Assert.AreEqual(Running, sequence.Tick(0.1f));
            Assert.AreEqual(Success, sequence.Tick(0.1f));
            Assert.AreEqual(1, first.Ticks);
        }

        [Test]
        public void Sequence_FailsAtOnceAndNextRoundStartsFromTheFirstChild()
        {
            ScriptedNode first = new(Success);
            ScriptedNode second = new(Failure);
            ScriptedNode third = new(Success);
            SequenceNode sequence = new(Children(first, second, third));

            Assert.AreEqual(Failure, sequence.Tick(0.1f));
            Assert.AreEqual(0, third.Ticks);

            sequence.Tick(0.1f);
            Assert.AreEqual(2, first.Ticks);
        }

        [Test]
        public void Selector_PicksTheFirstChildThatDoesNotFail()
        {
            ScriptedNode failing = new(Failure);
            ScriptedNode working = new(Running);
            ScriptedNode unused = new(Success);
            SelectorNode selector = new(Children(failing, working, unused));

            Assert.AreEqual(Running, selector.Tick(0.1f));
            Assert.AreEqual(1, failing.Ticks);
            Assert.AreEqual(0, unused.Ticks);
        }

        [Test]
        public void Selector_FailsWhenEveryChildFails()
        {
            SelectorNode selector = new(Children(new ScriptedNode(Failure), new ScriptedNode(Failure)));

            Assert.AreEqual(Failure, selector.Tick(0.1f));
        }

        [Test]
        public void Selector_ResetsTheRunningChildWhenAHigherPriorityOneTakesOver()
        {
            ScriptedNode urgent = new(Success, Failure, Success);
            ScriptedNode idle = new(Running);
            SelectorNode selector = new(Children(urgent, idle));

            Assert.AreEqual(Running, selector.Tick(0.1f), "urgent fails, idle runs");
            Assert.AreEqual(Success, selector.Tick(0.1f), "urgent preempts idle");

            Assert.AreEqual(1, idle.Resets);
        }

        [Test]
        public void Selector_ResetsTheRunningChildWhenEverythingFailsAfterwards()
        {
            ScriptedNode only = new(Failure, Running);
            SelectorNode selector = new(Children(only));

            selector.Tick(0.1f);
            Assert.AreEqual(Failure, selector.Tick(0.1f));

            Assert.AreEqual(1, only.Resets);
        }

        [Test]
        public void Initialize_ReachesEveryChildOfNestedNodes()
        {
            ScriptedNode leaf = new(Success);
            SequenceNode inner = new(Children(leaf));
            SelectorNode root = new(Children(inner));

            root.Initialize();

            Assert.AreEqual(1, leaf.Initializations);
        }

        [Test]
        public void Move_FailsWithoutAMovementModule()
        {
            MoveNode node = CreateMove(FakeActor.Prey(1), 1f, out _);

            Assert.AreEqual(Failure, node.Tick(0.1f));
        }

        [Test]
        public void Move_WithoutADurationKeepsMovingForever()
        {
            RecordingMovement movement = new();
            MoveNode node = CreateMove(FakeActor.Prey(1).With(movement), 0f, out FakeRoute route);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(Running, node.Tick(1f));
            }

            Assert.AreEqual(100, movement.Routes.Count);
            Assert.AreSame(route, movement.Routes[0]);
        }

        [Test]
        public void Move_WithADurationSucceedsOnceItHasMovedLongEnough()
        {
            RecordingMovement movement = new();
            MoveNode node = CreateMove(FakeActor.Prey(1).With(movement), 1f, out _);

            Assert.AreEqual(Running, node.Tick(0.5f));
            Assert.AreEqual(Success, node.Tick(0.5f));
            Assert.AreEqual(Running, node.Tick(0.5f), "starts a new leg after succeeding");
        }

        private static WaitNode CreateWait(float seconds, out RecordingMovement movement)
        {
            movement = new RecordingMovement();
            WaitNode node = new(FakeActor.Prey(1).With(movement), seconds);
            node.Initialize();
            return node;
        }

        private static MoveNode CreateMove(FakeActor owner, float duration, out FakeRoute route)
        {
            route = new FakeRoute();
            MoveNode node = new(owner, ScriptableObject.CreateInstance<FakeGait>(), route, duration);
            node.Initialize();
            return node;
        }
    }
}
