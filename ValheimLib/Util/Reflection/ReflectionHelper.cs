using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace ValheimLib.Util.Reflection
{
    public static class ReflectionHelper
    {
        public const BindingFlags AllBindingFlags = (BindingFlags) (-1);

        public static bool IsSameOrSubclass(this Type type, Type @base)
        {
            return type.IsSubclassOf(@base)
                   || type == @base;
        }

        public static bool IsEnumerable(this Type self)
        {
            return typeof(IEnumerable).IsAssignableFrom(self) && self != typeof(string);
        }

        // https://stackoverflow.com/a/21995826
        public static Type GetEnumeratedType(this Type type) =>
            type?.GetElementType() ?? 
            (typeof(IEnumerable).IsAssignableFrom(type) ? type.GetGenericArguments().FirstOrDefault() : null);

        public static class Cache
        {
            private static MethodInfo _enumerableToArray;
            public static MethodInfo EnumerableToArray
            {
                get
                {
                    if (_enumerableToArray == null)
                    {
                        _enumerableToArray = typeof(Enumerable).GetMethod("ToArray", AllBindingFlags);
                    }

                    return _enumerableToArray;
                }
            }

            private static MethodInfo _enumerableCast;
            public static MethodInfo EnumerableCast
            {
                get
                {
                    if (_enumerableCast == null)
                    {
                        _enumerableCast = typeof(Enumerable).GetMethod("Cast", AllBindingFlags);
                    }

                    return _enumerableCast;
                }
            }
        }
	}
}
