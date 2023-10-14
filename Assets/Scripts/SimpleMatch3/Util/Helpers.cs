using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleMatch3.Util
{
    public static class Helpers
    {
        public static T RandomEnum<T>()
        {
            var values = Enum.GetValues(typeof(T));
            var random = UnityEngine.Random.Range(0, values.Length);
            return (T) values.GetValue(random);
        }

        public static T RandomEnum<T>(List<T> excludeList)
        {
            var values = ((Enum.GetValues(typeof(T)) as T[]) ?? Array.Empty<T>()).ToList();
            foreach (var exclude in excludeList)
            {
                values.Remove(exclude);
            }
            
            var random = UnityEngine.Random.Range(0, values.Count);
            return values[random];
        }
    }
}