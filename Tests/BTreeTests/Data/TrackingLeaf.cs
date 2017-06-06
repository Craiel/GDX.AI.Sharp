namespace GDX.AI.Sharp.Tests.BTreeTests.Data
{
    using BTree;

    using Enums;

    public class TrackingLeaf : LeafTask<TestBlackboard>
    {
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void CopyTo(Task<TestBlackboard> clone)
        {
            throw new System.NotImplementedException();
        }

        protected override BTTaskStatus Execute()
        {
            this.Blackboard.Track(TestBlackboard.TrackingExecution);

            return BTTaskStatus.Succeeded;
        }
    }
}
