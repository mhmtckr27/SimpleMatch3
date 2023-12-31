﻿using System.Collections.Generic;

namespace SimpleMatch3.Extensions
{
    public static class ListExtensions
    {
        public static void AddUnique<T>(this List<T> list, T element)
        {
            if(!list.Contains(element))
                list.Add(element);
        }
    }
}