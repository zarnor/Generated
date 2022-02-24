using System.Collections.Generic;

namespace Generated.Builders;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> Append<T>(this IEnumerable<T> list, T element)
    {
        foreach (var item in list)
        {
            yield return item;
        }

        yield return element;
    }
}
