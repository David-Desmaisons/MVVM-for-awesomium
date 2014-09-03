using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesoniumPOC.Infra
{
    public static class EnumarableCollection
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T> ToDo)
        {
            foreach (T el in @this)
            {
                ToDo(el);
            }

            return @this;
        }

        public static IEnumerable<T> Single<T>(this T @this)
        {
            yield return @this;
        }
    }
}
