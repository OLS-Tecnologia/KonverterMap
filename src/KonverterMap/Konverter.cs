using KonverterMap.Utils;
using System.Collections;
using System.Reflection;

namespace KonverterMap
{
    public class Konverter
    {
        #region Properties

        private static Konverter? instance;
        private Dictionary<object, object> mappedTypes = new();
        private Dictionary<object, object> ignoredMembers = new();
        private Dictionary<(Type, Type, string), Delegate> customMappings = new();
        private HashSet<(Type, Type, string)> ignoredProperties = new();
        private Dictionary<(Type, Type, string), Func<object, bool>> conditionalMappings = new();

        public static Konverter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Konverter();
                    instance.mappedTypes = new Dictionary<object, object>();
                }
                return instance;
            }
        }

        public Dictionary<object, object> MappedTypes
        {
            get { return this.mappedTypes; }
            set { this.mappedTypes = value; }
        }

        public Dictionary<object, object> IgnoredMembers
        {
            get { return ignoredMembers; }
            set { ignoredMembers = value; }
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

        #endregion

        #region Private Methods

        private TDestination MapObject<TSource, TDestination>(
            TSource realObject,
            TDestination? dtoObject = default,
            Dictionary<object, object>? alreadyInitializedObjects = null,
            bool shouldMapInnerEntities = true)
            where TSource : class, new()
            where TDestination : class, new()
        {
            if (realObject == null)
                throw new ArgumentNullException("A classe de Origem não pode ser nula.");

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

                PropertyInfo? sourceProperty = sourceType.GetProperty(currentDtoProperty.Name);
                if (sourceProperty == null || !currentDtoProperty.CanWrite)
                    continue;

                if (MappedTypes.ContainsKey(sourceProperty.PropertyType) && shouldMapInnerEntities)
                {
                    object mapToObject = mappedTypes[sourceProperty.PropertyType];
                    var realObjectPropertyValue = sourceProperty.GetValue(realObject, null);

                    if (realObjectPropertyValue != null)
                    {
                        if (alreadyInitializedObjects.ContainsKey(realObjectPropertyValue))
                        {
                            currentDtoProperty.SetValue(dtoObject, alreadyInitializedObjects[realObjectPropertyValue]);
                        }
                        else
                        {
                            alreadyInitializedObjects.Add(realObjectPropertyValue, null);
                            var newProxyProperty = ExecuteMap(new Type[] { sourceProperty.PropertyType, (Type)mapToObject }, realObjectPropertyValue);
                            currentDtoProperty.SetValue(dtoObject, newProxyProperty);
                            alreadyInitializedObjects[realObjectPropertyValue] = newProxyProperty;
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
                            Type customList = typeof(List<>).MakeGenericType(ReflectionUtils.ExtractElementType(currentDtoProperty.PropertyType));
                            IList? objectList = (IList?)Activator.CreateInstance(customList);
                            IList? copyfrom = (IList?)sourceProperty.GetValue(realObject, null);

                            for (int y = 0; y < copyfrom?.Count; y++)
                            {
                                object mapToObject = mappedTypes[sourceProperty.PropertyType.GetGenericArguments()[0]];
                                objectList?.Add(ExecuteMap(new Type[] { sourceProperty.PropertyType.GetGenericArguments()[0], (Type)mapToObject }, copyfrom[y]));
                            }

                            currentDtoProperty.SetValue(dtoObject, objectList);
                        }
                    }
                    else
                    {
                        currentDtoProperty.SetValue(dtoObject, sourceProperty.GetValue(realObject, null));
                    }
                }
            }

            return dtoObject;
        }

        private TDestination? GetMapper<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null)
                throw new ArgumentNullException("O objeto a ser mapeado não pode ser nulo.");

            Type srcType = typeof(TSource);
            if (ReflectionUtils.IsCollection(srcType))
            {
                IList? destinationList = (IList?)destination;
                IList copyfrom = (IList)source;

                for (int i = 0; i < copyfrom.Count; i++)
                    destinationList?.Add(ExecuteMap(new Type[] { ReflectionUtils.ExtractElementType(srcType), ReflectionUtils.ExtractElementType(typeof(TDestination)) }, copyfrom[i]));

                destination = (TDestination)destinationList!;
            }
            else
                destination = (TDestination)ExecuteMap(new Type[] { ReflectionUtils.ExtractElementType(srcType), ReflectionUtils.ExtractElementType(typeof(TDestination)) }, source);

            return destination;
        }

        private static object? GetDestination(Type destinationType)
        {
            if (ReflectionUtils.IsCollection(destinationType))
            {
                Type ListType = typeof(List<>).MakeGenericType(ReflectionUtils.ExtractElementType(destinationType));
                IList? destinationList = (IList?)Activator.CreateInstance(ListType);
                return destinationList;
            }
            else
            {
                object? destination = Activator.CreateInstance(destinationType);
                return destination;
            }
        }

        private object? ExecuteMap(Type[] types, object item)
        {
            MethodInfo method = GetType().GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(types);
            var objects = new object[] {
                item,
                null,
                new Dictionary<object, object>(),
                true
            };
            return method.Invoke(this, objects);
        }

        #endregion

        #region Public Methods

        public MapConfig<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TSource : new()
            where TDestination : new()
        {
            if (!MappedTypes.ContainsKey(typeof(TSource)))
            {
                MappedTypes.Add(typeof(TSource), typeof(TDestination));
            }

            return new MapConfig<TSource, TDestination>(this);
        }

        public MapConfig<TDestination, TSource> ReverseMap<TSource, TDestination>()
            where TSource : new()
            where TDestination : new()
        {
            return CreateMap<TDestination, TSource>();
        }

        public TDestination? Map<TSource, TDestination>(TSource source)
        {
            return GetMapper(source, (TDestination?)GetDestination(typeof(TDestination)));
        }

        #endregion
    }
}