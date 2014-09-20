﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Infra
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

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T,int> ToDo)
        {
            int i=0;
            foreach (T el in @this)
            {
                ToDo(el,i++);
            }

            return @this;
        }
    }
}
