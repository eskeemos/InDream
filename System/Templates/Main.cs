using System.Collections.Generic;

namespace System.Templates
{
    public class Main
    {
        public int StrategyId { get; set; }
        public int Interval { get; set; }
        public bool Test { get; set; }
        public IEnumerable<Strategy> Strategies { get; set; }
    }
}
