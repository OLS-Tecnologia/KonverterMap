using System.Collections;
using System.Reflection;

namespace KonverterMap.Utils
{
    public static class ReflectionUtils
    {
        public static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsPrimitive(Type type)
        {
            return
                type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(Guid)
                || type == typeof(float)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(short)
                || type == typeof(DateTime)
                || (IsNullable(type) && IsPrimitive(Nullable.GetUnderlyingType(type)!))
                || type.IsEnum;
        }

        public static bool IsCollection(Type type)
        {
            return type != typeof(byte[]) &&
                type.IsArray ||
                (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)
                                     || type.GetGenericTypeDefinition() == typeof(ICollection<>)
                                     || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                     || type.GetGenericTypeDefinition() == typeof(IList<>)))
                || type == typeof(ArrayList)
                || typeof(IList).IsAssignableFrom(type)
                || (type.IsGenericType && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        public static MethodInfo? CreatePrimitiveConverter(Type sourceType, Type destinationType)
        {
            Type srcType = IsNullable(sourceType)
                ? sourceType.GetGenericArguments()[0]
                : sourceType;

            Type destType = IsNullable(destinationType)
                ? destinationType.GetGenericArguments()[0]
                : destinationType;

            if (srcType == destType)
                return null;

            if (destType == typeof(string))
                return typeof(Convert).GetMethod("ToString", new[] { typeof(object) });

            if (destType == typeof(bool))
                return typeof(Convert).GetMethod("ToBoolean", new[] { typeof(object) });

            if (destType == typeof(int))
                return typeof(Convert).GetMethod("ToInt32", new[] { typeof(object) });

            if (destType == typeof(uint))
                return typeof(Convert).GetMethod("ToUInt32", new[] { typeof(object) });

            if (destType == typeof(byte))
                return typeof(Convert).GetMethod("ToByte", new[] { typeof(object) });

            if (destType == typeof(sbyte))
                return typeof(Convert).GetMethod("ToSByte", new[] { typeof(object) });

            if (destType == typeof(long))
                return typeof(Convert).GetMethod("ToInt64", new[] { typeof(object) });

            if (destType == typeof(ulong))
                return typeof(Convert).GetMethod("ToUInt64", new[] { typeof(object) });

            if (destType == typeof(short))
                return typeof(Convert).GetMethod("ToInt16", new[] { typeof(object) });

            if (destType == typeof(ushort))
                return typeof(Convert).GetMethod("ToUInt16", new[] { typeof(object) });

            if (destType == typeof(decimal))
                return typeof(Convert).GetMethod("ToDecimal", new[] { typeof(object) });

            if (destType == typeof(double))
                return typeof(Convert).GetMethod("ToDouble", new[] { typeof(object) });

            if (destType == typeof(float))
                return typeof(Convert).GetMethod("ToSingle", new[] { typeof(object) });

            if (destType == typeof(DateTime))
                return typeof(Convert).GetMethod("ToDateTime", new[] { typeof(object) });

            if (destType == typeof(Guid))
                return typeof(ReflectionUtils).GetMethod("ConvertToGuid", new[] { typeof(object) });

            return null;
        }

        public static Type ExtractElementType(Type collection)
        {
            if (collection.IsArray)
            {
                return collection.GetElementType()!;
            }

            if (collection == typeof(ArrayList))
            {
                return typeof(object);
            }

            if (collection.IsGenericType)
            {
                return collection.GetGenericArguments()[0];
            }

            return collection;
        }
    }
}
