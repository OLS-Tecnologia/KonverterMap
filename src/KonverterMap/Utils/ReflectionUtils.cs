using System.Collections;
using System.Linq.Expressions;
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
            if (type == typeof(string))
                return false;

            if (type.IsArray)
                return true;

            if (!type.IsGenericType)
                return false;

            var genericTypeDef = type.GetGenericTypeDefinition();

            return typeof(IEnumerable).IsAssignableFrom(type) &&
                   type != typeof(string);
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
            if (!IsCollection(collection))
                throw new InvalidOperationException($"Type '{collection.Name}' is not a recognized collection.");

            if (collection.IsArray)
                return collection.GetElementType()!;

            if (collection == typeof(ArrayList))
                return typeof(object);

            if (collection.IsGenericType)
                return collection.GetGenericArguments()[0];

            throw new InvalidOperationException($"Unable to extract element type from '{collection.Name}'.");
        }

        public static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression innerMember)
            {
                return innerMember.Member.Name;
            }

            throw new ArgumentException("Invalid Expression. Expected direct access to a property.");
        }
    }
}
