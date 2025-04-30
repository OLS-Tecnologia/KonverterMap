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
            string? propertyNameBeingMapped = null;
            Type? sourcePropertyType = null;
            Type? destinationPropertyType = null;

            try
            {
                if (realObject == null)
                    throw new ArgumentNullException(nameof(realObject));

                alreadyInitializedObjects ??= new Dictionary<object, object>();
                dtoObject ??= new TDestination();
                alreadyInitializedObjects[realObject] = dtoObject;

                // Executa BeforeMap (se houver)
                if (beforeMaps.TryGetValue((typeof(TSource), typeof(TDestination)), out var beforeAction))
                {
                    beforeAction(realObject, dtoObject);
                }

                var sourceType = typeof(TSource);
                var destinationType = typeof(TDestination);

                PropertyInfo[] destinationProperties = destinationType.GetProperties();

                foreach (var currentDtoProperty in destinationProperties)
                {
                    propertyNameBeingMapped = currentDtoProperty.Name;
                    destinationPropertyType = currentDtoProperty.PropertyType;

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

                    if (sourceProperty?.GetValue(realObject) == realObject)
                    {
                        currentDtoProperty.SetValue(dtoObject, dtoObject);
                        continue;
                    }

                    sourcePropertyType = sourceProperty.PropertyType;
                    var sourceValue = sourceProperty.GetValue(realObject);

                    if (MappedTypes.ContainsKey(sourceProperty.PropertyType) && shouldMapInnerEntities)
{
                        if (sourceValue == null)
                        {
                            currentDtoProperty.SetValue(dtoObject, null);
                            continue;
                        }

                        // Se já foi mapeado anteriormente (previne ciclo)
                        if (alreadyInitializedObjects.TryGetValue(sourceValue, out var cached))
                        {
                            currentDtoProperty.SetValue(dtoObject, cached);
                            continue;
                        }

                        // Se tipo de origem e destino são idênticos, fazer cópia direta
                        if (sourceProperty.PropertyType == currentDtoProperty.PropertyType)
                        {
                            if (MappedTypes.ContainsKey(sourceProperty.PropertyType))
                            {
                                if (alreadyInitializedObjects.TryGetValue(sourceValue, out var cachedobj))
                                {
                                    currentDtoProperty.SetValue(dtoObject, cachedobj);
                                    continue;
                                }

                                alreadyInitializedObjects[sourceValue] = null!;
                                var newValueobj = ExecuteMap(new[] {
                                    sourceProperty.PropertyType,
                                    currentDtoProperty.PropertyType
                                }, sourceValue, alreadyInitializedObjects);
                                currentDtoProperty.SetValue(dtoObject, newValueobj);
                                alreadyInitializedObjects[sourceValue] = newValueobj;
                                continue;
                            }

                            // Tipo igual e não mapeável: cópia direta
                            currentDtoProperty.SetValue(dtoObject, sourceValue);
                            continue;
                        }

                        // Mapeia usando ExecuteMap com controle de ciclo
                        var mapToObject = MappedTypes[sourceProperty.PropertyType];
                        alreadyInitializedObjects[sourceValue] = null!; // marca como sendo processado
                        var newValue = ExecuteMap(new[] { sourceProperty.PropertyType, (Type)mapToObject }, sourceValue, alreadyInitializedObjects);
                        currentDtoProperty.SetValue(dtoObject, newValue);
                        alreadyInitializedObjects[sourceValue] = newValue;
                        continue;
                    }

                    if (ReflectionUtils.IsCollection(sourceProperty.PropertyType))
                    {
                        var sourceList = sourceValue as IList;
                        if (sourceList == null)
                        {
                            currentDtoProperty.SetValue(dtoObject, null);
                            continue;
                        }

                        var sourceElementType = ReflectionUtils.ExtractElementType(sourceProperty.PropertyType);
                        var destinationElementType = ReflectionUtils.ExtractElementType(currentDtoProperty.PropertyType);

                        // Se tipos forem iguais, faz cópia direta da lista
                        if (sourceElementType == destinationElementType)
                        {
                            var copiedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(destinationElementType)) as IList;
                            foreach (var item in sourceList)
                            {
                                copiedList!.Add(item);
                            }

                            currentDtoProperty.SetValue(dtoObject, copiedList);
                            continue;
                        }

                        // Se tipo estiver mapeado, faz mapeamento item a item com proteção contra ciclo
                        if (MappedTypes.ContainsKey(sourceElementType))
                        {
                            var mapToObject = MappedTypes[sourceElementType];
                            var customList = typeof(List<>).MakeGenericType(destinationElementType);
                            var objectList = (IList)Activator.CreateInstance(customList)!;

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
                                    continue;
                                }

                                alreadyInitializedObjects[item] = null!;
                                var mappedItem = ExecuteMap(new[] { sourceElementType, (Type)mapToObject }, item, alreadyInitializedObjects);
                                objectList.Add(mappedItem);
                                alreadyInitializedObjects[item] = mappedItem;
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
            catch (IndexOutOfRangeException ex)
            {
                var sourceType = realObject?.GetType()?.FullName ?? "null";
                var destinationType = dtoObject?.GetType()?.FullName ?? "null";

                throw new KonverterMappingException(
                                    $"Error to map from {sourceType} to {destinationType}. " +
                                    $"Property: {propertyNameBeingMapped ?? "Unknown"} " +
                                    $"(SourceType: {sourcePropertyType?.FullName ?? "Unknown"}, " +
                                    $"DestinationType: {destinationPropertyType?.FullName ?? "Unknown"}).",
                                    ex);
            }
            catch (Exception ex)
            {
                var sourceType = realObject?.GetType()?.FullName ?? "null";
                var destinationType = dtoObject?.GetType()?.FullName ?? "null";

                throw new KonverterMappingException(
                                    $"Unexpected error to map from {sourceType} to {destinationType}. " +
                                    $"Property: {propertyNameBeingMapped ?? "Unknown"} " +
                                    $"(SourceType: {sourcePropertyType?.FullName ?? "Unknown"}, " +
                                    $"DestinationType: {destinationPropertyType?.FullName ?? "Unknown"}).",
                                    ex);
            }
        }

        private TDestination GetMapper<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Type srcType = typeof(TSource);
            Type dstType = typeof(TDestination);

            if (ReflectionUtils.IsCollection(srcType))
            {
                var destinationList = (IList)(object)destination!;
                var sourceList = (IList)(object)source!;

                Type srcElementType = ReflectionUtils.ExtractElementType(srcType);
                Type dstElementType = ReflectionUtils.ExtractElementType(dstType);

                foreach (var item in sourceList)
                {
                    var mappedItem = ExecuteMap(new[] { srcElementType, dstElementType }, item);
                    destinationList.Add(mappedItem);
                }

                return (TDestination)destinationList;
            }

            // Aqui o tipo não é coleção — usamos os próprios tipos diretamente
            return (TDestination)ExecuteMap(new[] { srcType, dstType }, source);
        }

        private static object GetDestination(Type destinationType)
        {
            if (ReflectionUtils.IsCollection(destinationType))
            {
                Type elementType = ReflectionUtils.ExtractElementType(destinationType);
                Type listType = typeof(List<>).MakeGenericType(elementType);

                // Garante que a List<T> é atribuível ao tipo esperado
                if (!destinationType.IsAssignableFrom(listType))
                {
                    throw new InvalidOperationException($"The destination type '{destinationType.FullName}' is not compatible with List<{elementType.Name}>.");
                }

                return Activator.CreateInstance(listType)
                       ?? throw new InvalidOperationException("Failed to create collection instance.");
            }

            return Activator.CreateInstance(destinationType)
                   ?? throw new InvalidOperationException($"Failed to create instance of type '{destinationType.FullName}'.");
        }

        private object ExecuteMap(Type[] types, object item)
        {
            return ExecuteMap(types, item, new Dictionary<object, object>());
        }

        private object ExecuteMap(Type[] types, object item, Dictionary<object, object> alreadyInitializedObjects)
        {
            try
            {
                var method = GetType().GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(types);

                var args = new object[] { item, null!, new Dictionary<object, object>(), true };
                return method.Invoke(this, args)!;
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                throw tie.InnerException;
            }
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