using KonverterMap.Utils;
using System;
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
        private readonly Dictionary<(Type, Type), Action<object, object>> beforeMaps = new();
        private readonly Dictionary<(Type, Type), Action<object, object>> afterMaps = new();
        private readonly Dictionary<(Type, Type), List<Action>> _typeConfigurations = new();

        public static Konverter Instance
        {
            get
            {
                if (instance == null)
                    instance = new Konverter();
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

        internal void StoreConfig<TSource, TDestination>(Action<MapConfig<TSource, TDestination>> config)
            where TSource : new()
            where TDestination : new()
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (!_typeConfigurations.TryGetValue(key, out var actions))
            {
                actions = new List<Action>();
                _typeConfigurations[key] = actions;
            }

            actions.Add(() =>
            {
                var cfg = new MapConfig<TSource, TDestination>(this);
                config(cfg);
            });
        }

        private TDestination MapObject<TSource, TDestination>(
            TSource realObject,
            TDestination? dtoObject = null,
            Dictionary<object, object>? alreadyInitializedObjects = null,
            bool shouldMapInnerEntities = true)
            where TSource : class, new()
            where TDestination : class, new()
        {

            if (realObject == null)
                throw new ArgumentNullException(nameof(realObject));

            alreadyInitializedObjects ??= new Dictionary<object, object>();
            dtoObject ??= new TDestination();

            // Executa BeforeMap (se houver)
            if (beforeMaps.TryGetValue((typeof(TSource), typeof(TDestination)), out var beforeAction))
            {
                beforeAction?.Invoke(realObject, dtoObject);
            }

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

                    if (sourceValue == null)
                    {
                        currentDtoProperty.SetValue(dtoObject, null);
                        continue;
                    }

                    if (alreadyInitializedObjects.TryGetValue(sourceValue, out var cached))
                    {
                        currentDtoProperty.SetValue(dtoObject, cached);
                        continue;
                    }

                    alreadyInitializedObjects[sourceValue] = null!;
                    var newValue = ExecuteMap(new[] { sourceProperty.PropertyType, (Type)mapToObject }, sourceValue);
                    currentDtoProperty.SetValue(dtoObject, newValue);
                    alreadyInitializedObjects[sourceValue] = newValue;
                    continue;
                }

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
                                if (item == null)
                                {
                                    objectList.Add(null);
                                    continue;
                                }

                                if (alreadyInitializedObjects.TryGetValue(item, out var cached))
                                {
                                    objectList.Add(cached);
                                }
                                else
                                {
                                    var mapToObject = mappedTypes[sourceProperty.PropertyType.GetGenericArguments()[0]];
                                    alreadyInitializedObjects[item] = null!;
                                    var mappedItem = ExecuteMap(new[] {
                                        sourceProperty.PropertyType.GetGenericArguments()[0],
                                        (Type)mapToObject
                                    }, item);
                                    objectList.Add(mappedItem);
                                    alreadyInitializedObjects[item] = mappedItem;
                                }
                            }
                        }

                        currentDtoProperty.SetValue(dtoObject, objectList);
                        continue;
                    }
                }

                currentDtoProperty.SetValue(dtoObject, sourceValue);
            }

            // Executa AfterMap (se houver)
            if (afterMaps.TryGetValue((typeof(TSource), typeof(TDestination)), out var afterAction))
            {
                afterAction(realObject, dtoObject);
            }

            return dtoObject;
        }

        private TDestination GetMapper<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

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
                return Activator.CreateInstance(listType) ?? throw new InvalidOperationException("Falha ao criar instância.");
            }

            return Activator.CreateInstance(destinationType) ?? throw new InvalidOperationException("Falha ao criar instância.");
        }

        private object ExecuteMap(Type[] types, object item)
        {
            var method = GetType().GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(types);

            var args = new object[] { item, null!, new Dictionary<object, object>(), true };
            return method.Invoke(this, args)!;
        }

        internal void RegisterBeforeMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            beforeMaps[(typeof(TSource), typeof(TDestination))] = (src, dest) => action((TSource)src, (TDestination)dest);
        }

        internal void RegisterAfterMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            afterMaps[(typeof(TSource), typeof(TDestination))] = (src, dest) => action((TSource)src, (TDestination)dest);
        }

        public MapConfig<TSource, TDestination> CreateMap<TSource, TDestination>(bool reverse = false)
            where TSource : new()
            where TDestination : new()
        {
            if (!MappedTypes.ContainsKey(typeof(TSource)))
            {
                MappedTypes[typeof(TSource)] = typeof(TDestination);
            }

            if (reverse)
            {
                var originalKey = (typeof(TDestination), typeof(TSource));
                if (_typeConfigurations.TryGetValue(originalKey, out var configs))
                {
                    foreach (var config in configs)
                    {
                        config();
                    }
                }
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