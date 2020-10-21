using System.Collections.Generic;
using System.Linq;

namespace SharpBrick.PoweredUp.BlueZ.Utilities
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> NullToEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? Enumerable.Empty<TSource>();
        }
    }
}