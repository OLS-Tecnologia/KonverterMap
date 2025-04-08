using KonverterMap.Utils;
using System.Collections;
using System.Reflection;

namespace KonverterMap
{
    public class Konverter
    {
        private static Konverter? instance;
        private Dictionary<object, object> mappedTypes = new();
        private Dictionary<object, object> ignoredMembers = new();
        private readonly Dictionary<(Type, Type, string), Delegate> customMappings = new();
        private readonly HashSet<(Type, Type, string)> ignoredProperties = new();
        private readonly Dictionary<(Type, Type, string), Func<object, bool>> conditionalMappings = new();

        public static Konverter Instance
        {
            get
            {
                instance ??= new Konverter();
                return instance;
            }
        }

        public Dictionary<object, object> MappedTypes
        {
            get => mappedTypes;
            set => mappedTypes = value;
        }

        public Dictionary<object, object> IgnoredMembers
        {
            get => ignoredMembers;
            set => ignoredMembers = value;
        }

        internal void RegisterCustomMapping((Type, Type, string) key, Delegate map)
        {
            customMappings[key] = map;
        }

        internal void RegisterIgnoredProperty((Type, Type, string) key)
        {
            ignoredProperties.Add(key);
        }

        internal void RegisterConditionalMapping((Type, Type, string) key, Func<object, bool> condition)
        {
            conditionalMappings[key] = condition;
        }

        private TDestination MapObject<TSource, TDestination>(
            TSource realObject,
            TDestination? dtoObject = null,
            Dictionary<object, object>? alreadyInitializedObjects = null,
            bool shouldMapInnerEntities = true)
            where TSource : class, new()
            where TDestination : class, new()
        {
            ArgumentNullException.ThrowIfNull(realObject);

            alreadyInitializedObjects ??= new Dictionary<object, object>();
            dtoObject ??= new TDestination();

            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            PropertyInfo[] destinationProperties = destinationType.GetProperties();

            foreach (var currentDtoProperty in destinationProperties)
            {
                var key = (sourceType, destinationType, currentDtoProperty.Name);

                if (ignoredProperties.Contains(key))
                    continue;

                if (conditionalMappings.TryGetValue(key, out var condition) && !condition(realObject))
                    continue;

                if (customMappings.TryGetValue(key, out var customMap))
                {
                    var mappedValue = customMap.DynamicInvoke(realObject);
                    currentDtoProperty.SetValue(dtoObject, mappedValue);
                    continue;
                }

                var sourceProperty = sourceType.GetProperty(currentDtoProperty.Name);
                if (sourceProperty == null || !currentDtoProperty.CanWrite)
                    continue;

                var sourceValue = sourceProperty.GetValue(realObject);
                if (MappedTypes.ContainsKey(sourceProperty.PropertyType) && shouldMapInnerEntities)
                {
                    var mapToObject = mappedTypes[sourceProperty.PropertyType];
                    if (sourceValue != null)
                    {
                        if (alreadyInitializedObjects.TryGetValue(sourceValue, out var cached))
                        {
                            currentDtoProperty.SetValue(dtoObject, cached);
                        }
                        else
                        {
                            alreadyInitializedObjects[sourceValue] = null!;
                            var newValue = ExecuteMap(new[] { sourceProperty.PropertyType, (Type)mapToObject }, sourceValue);
                            currentDtoProperty.SetValue(dtoObject, newValue);
                            alreadyInitializedObjects[sourceValue] = newValue;
                        }
                    }
                    else
                    {
                        currentDtoProperty.SetValue(dtoObject, null);
                    }
                }
                else
                {
                    if (ReflectionUtils.IsCollection(sourceProperty.PropertyType))
                    {
                        if (MappedTypes.ContainsKey(sourceProperty.PropertyType.GetGenericArguments()[0]))
                        {
                            var elementType = ReflectionUtils.ExtractElementType(currentDtoProperty.PropertyType);
                            var customList = typeof(List<>).MakeGenericType(elementType);
                            var objectList = (IList)Activator.CreateInstance(customList)!;
                            var sourceList = sourceValue as IList;

                            if (sourceList != null)
                            {
                                foreach (var item in sourceList)
                                {
                                    var mapToObject = mappedTypes[sourceProperty.PropertyType.GetGenericArguments()[0]];
                                    objectList.Add(ExecuteMap(new[] { sourceProperty.PropertyType.GetGenericArguments()[0], (Type)mapToObject }, item));
                                }
                            }

                            currentDtoProperty.SetValue(dtoObject, objectList);
                        }
                    }
                    else
                    {
                        currentDtoProperty.SetValue(dtoObject, sourceValue);
                    }
                }
            }

            return dtoObject;
        }

        private TDestination GetMapper<TSource, TDestination>(TSource source, TDestination destination)
        {
            ArgumentNullException.ThrowIfNull(source);

            Type srcType = typeof(TSource);
            if (ReflectionUtils.IsCollection(srcType))
            {
                var destinationList = (IList)(object)destination!;
                var sourceList = (IList)(object)source!;

                foreach (var item in sourceList)
                {
                    destinationList.Add(ExecuteMap(new[] {
                        ReflectionUtils.ExtractElementType(srcType),
                        ReflectionUtils.ExtractElementType(typeof(TDestination)) }, item));
                }

                return (TDestination)destinationList;
            }

            return (TDestination)ExecuteMap(new[] {
                ReflectionUtils.ExtractElementType(srcType),
                ReflectionUtils.ExtractElementType(typeof(TDestination)) }, source);
        }

        private static object GetDestination(Type destinationType)
        {
            if (ReflectionUtils.IsCollection(destinationType))
            {
                Type listType = typeof(List<>).MakeGenericType(ReflectionUtils.ExtractElementType(destinationType));
                return Activator.CreateInstance(listType)!;
            }

            return Activator.CreateInstance(destinationType)!;
        }

        private object ExecuteMap(Type[] types, object item)
        {
            var method = GetType().GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(types);

            var args = new object[] { item, null!, new Dictionary<object, object>(), true };
            return method.Invoke(this, args)!;
        }

        public MapConfig<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TSource : new()
            where TDestination : new()
        {
            if (!MappedTypes.ContainsKey(typeof(TSource)))
            {
                MappedTypes[typeof(TSource)] = typeof(TDestination);
            }

            return new MapConfig<TSource, TDestination>(this);
        }

        public MapConfig<TDestination, TSource> ReverseMap<TSource, TDestination>()
            where TSource : new()
            where TDestination : new()
        {
            return CreateMap<TDestination, TSource>();
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return GetMapper(source, (TDestination)GetDestination(typeof(TDestination)));
        }
    }
}