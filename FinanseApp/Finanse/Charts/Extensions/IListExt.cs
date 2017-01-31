using System;
using System.Collections;
using System.Collections.Generic;

namespace Finanse.Charts.Extensions
{
    internal static class IListExt
    {
        public static IEnumerable<T> Select<T>(this IList list, Func<object, T> func)
        {
            foreach (var item in list)
                yield return func(item);
        }
    }
}
