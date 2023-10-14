using System;
using System.Collections.Generic;
using System.Linq;

namespace Util.Extensions
{
    public static class TypeExtensions
    {
        // public static Type[] GetDerivedTypes(this Type baseType)
        // {
        //     return baseType.Assembly
        //         .GetTypes()
        //         .Where(t =>
        //             t.BaseType is { IsGenericType: false } &&
        //             t.BaseType.GetGenericTypeDefinition() == baseType).ToArray();
        // }
        
        public static Type[] GetDerivedTypes(this Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(baseType)).ToArray();
        }
    }
}