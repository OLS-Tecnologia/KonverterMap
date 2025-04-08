using KonverterMap;
using System.Linq.Expressions;
public static class MapConfigExtensions
{
    public static MapConfig<TSource, TDestination> ForMember<TSource, TDestination, TMember>(
        this MapConfig<TSource, TDestination> config,
        Expression<Func<TDestination, TMember>> destinationMember,
        Func<TSource, TMember> mappingFunc)
        where TSource : new()
        where TDestination : new()
    {
        var memberName = ((MemberExpression)destinationMember.Body).Member.Name;
        var key = (typeof(TSource), typeof(TDestination), memberName);
        Konverter.Instance.RegisterCustomMapping(key, mappingFunc);
        return config;
    }

    public static MapConfig<TSource, TDestination> Ignore<TSource, TDestination, TMember>(
        this MapConfig<TSource, TDestination> config,
        Expression<Func<TDestination, TMember>> destinationMember)
        where TSource : new()
        where TDestination : new()
    {
        var memberName = ((MemberExpression)destinationMember.Body).Member.Name;
        var key = (typeof(TSource), typeof(TDestination), memberName);
        Konverter.Instance.RegisterIgnoredProperty(key);
        return config;
    }

    public static MapConfig<TSource, TDestination> When<TSource, TDestination, TMember>(
        this MapConfig<TSource, TDestination> config,
        Expression<Func<TDestination, TMember>> destinationMember,
        Func<TSource, bool> condition)
        where TSource : new()
        where TDestination : new()
    {
        var memberName = ((MemberExpression)destinationMember.Body).Member.Name;
        var key = (typeof(TSource), typeof(TDestination), memberName);
        Konverter.Instance.RegisterConditionalMapping(key, src => condition((TSource)src));
        return config;
    }
}
