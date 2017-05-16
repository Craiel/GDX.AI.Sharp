namespace GDX.AI.Sharp.Tests.BTreeTests
{
    using BTree;
    using BTree.Decorators;

    using Data;

    using Enums;

    using NUnit.Framework;

    [TestFixture]
    public class DecoratorTests
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [Test]
        public void AlwaysFailTest()
        {
            var blackboard = new TestBlackboard();
            var node = new AlwaysFail<TestBlackboard>();
            var tree = BehaviorTreeBuilder<TestBlackboard>.Begin()
                .Sequence()
                    .Decorator(node).Leaf(new TrackingLeaf())
            .Build(blackboard);

            this.UpdateAndStep(tree, 0.1f);

            Assert.AreEqual(BTTaskStatus.Failed, node.Status);
            Assert.AreEqual(1, blackboard.GetTracking(TestBlackboard.TrackingExecution));
        }

        [Test]
        public void AlwaysSucceedTest()
        {
            var blackboard = new TestBlackboard();
            var node = new AlwaysSucceed<TestBlackboard>();
            var tree = BehaviorTreeBuilder<TestBlackboard>.Begin()
                .Sequence()
                    .Decorator(node).Leaf(new TrackingLeaf())
            .Build(blackboard);

            this.UpdateAndStep(tree, 0.1f);

            Assert.AreEqual(BTTaskStatus.Succeeded, node.Status);
            Assert.AreEqual(1, blackboard.GetTracking(TestBlackboard.TrackingExecution));
        }

        [Test]
        public void IncludeTest()
        {
            //Assert.Fail("Not Implemented");
        }

        [Test]
        public void IntervalTest()
        {
            var blackboard = new TestBlackboard();
            var tree = BehaviorTreeBuilder<TestBlackboard>.Begin()
                .Sequence()
                    .Interval().Leaf(new TrackingLeaf())
            .Build(blackboard);

            for (var n = 0; n < 10; n++)
            {
                for (var i = 0; i < 9; i++)
                {
                    this.UpdateAndStep(tree, 0.1f);
                }

                Assert.AreEqual(n, blackboard.GetTracking(TestBlackboard.TrackingExecution));

                this.UpdateAndStep(tree, 0.1f);

                Assert.AreEqual(n + 1, blackboard.GetTracking(TestBlackboard.TrackingExecution));
            }
        }

        [Test]
        public void InvertTest()
        {
            var blackboard = new TestBlackboard();
            var node = new Invert<TestBlackboard>();
            var tree = BehaviorTreeBuilder<TestBlackboard>.Begin()
                .Sequence()
                    .Decorator(node).Leaf(new TrackingLeaf())
            .Build(blackboard);

            this.UpdateAndStep(tree, 0.1f);

            Assert.AreEqual(BTTaskStatus.Failed, node.Status);
            Assert.AreEqual(1, blackboard.GetTracking(TestBlackboard.TrackingExecution));
        }

        [Test]
        public void RandomTest()
        {
            var blackboard = new TestBlackboard();
            var node = new Random<TestBlackboard>();
            var tree = BehaviorTreeBuilder<TestBlackboard>.Begin()
                .Sequence()
                    .Decorator(node).Leaf(new TrackingLeaf())
            .Build(blackboard);

            ushort[] result = new ushort[2];
            for (var i = 0; i < 100; i++)
            {
                this.UpdateAndStep(tree, 0.1f);
                if (node.Status == BTTaskStatus.Succeeded)
                {
                    result[1]++;
                }
                else
                {
                    result[0]++;
                }
            }

            // We should get a decently even spread but we can't rely on it, 50% sway should be plenty enough to have a stable result
            Assert.Greater(result[0], 25);
            Assert.Greater(result[1], 25);

            Assert.AreEqual(100, blackboard.GetTracking(TestBlackboard.TrackingExecution));
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void UpdateAndStep(BehaviorStream<TestBlackboard> stream, float delta)
        {
            GDXAI.TimePiece.Update(delta);
            stream.Step();
        }
    }
}
