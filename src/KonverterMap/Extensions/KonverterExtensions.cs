using System.Reflection;

namespace KonverterMap.Extensions
{
    public static class KonverterExtensions
    {
        public static void MapInto<TSource, TDestination>(this Konverter konverter, TSource source, TDestination destination)
        {
            if (konverter == null)
                throw new ArgumentNullException(nameof(konverter));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            konverter.GetType()
                     .GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic)!
                     .MakeGenericMethod(typeof(TSource), typeof(TDestination))
                     .Invoke(konverter, new object[] { source, destination, null!, true });
        }
    }
}
