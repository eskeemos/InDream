using System.Collections.Generic;
using System.Linq;

namespace Scheme.Actions
{
    public class Average
    {
        public static decimal GetAverage(IEnumerable<decimal> values, int amount = 0)
            => amount == 0
                ? values.Average()
                : values.Count() > amount ? values.TakeLast(amount).Average() : values.Average();
    }
}
