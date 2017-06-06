namespace GDX.AI.Sharp.Tests.BTreeTests.Data
{
    using System.Collections.Generic;

    using Contracts;

    public class TestBlackboard : IBlackboard
    {
        public const string TrackingExecution = "Execution";

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public TestBlackboard()
        {
            this.Trackers = new Dictionary<string, int>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IDictionary<string, int> Trackers { get; set; }

        public void Track(string key, int value = 1)
        {
            if (!this.Trackers.ContainsKey(key))
            {
                this.Trackers.Add(key, 0);
            }

            this.Trackers[key] += value;
        }

        public int GetTracking(string key)
        {
            int result;
            if (this.Trackers.TryGetValue(key, out result))
            {
                return result;
            }

            return 0;
        }
    }
}
