using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheme.Extensions
{
    public static class Collection
    {
        public static bool AnyAndNotNull<T>(this IEnumerable<T> source)
            => source != null && source.Any();
    }
}
