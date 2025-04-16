using KonverterMap;
using KonverterMap.Utils;
using System.Linq.Expressions;
public class MapConfig<TSource, TDestination>
        where TSource : new()
        where TDestination : new()
{
    private readonly Konverter konverter;

    public MapConfig(Konverter konverter)
    {
        this.konverter = konverter;
    }

    public Konverter And() => konverter;

    public MapConfig<TDestination, TSource> ReverseMap()
    {
        return konverter.ReverseMap<TSource, TDestination>();
    }

    public MapConfig<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Func<TSource, Konverter, TMember> mapFunc)
    {
        konverter.StoreConfig<TSource, TDestination>(cfg =>
            cfg.ForMember(destinationMember, mapFunc)
        );

        var memberName = ReflectionUtils.GetMemberName(destinationMember);
        var key = (typeof(TSource), typeof(TDestination), memberName);
        konverter.RegisterCustomMapping(key, (Func<TSource, TMember>)(src => mapFunc(src, konverter)));

        return this;
    }

    public MapConfig<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember)
    {
        konverter.StoreConfig<TSource, TDestination>(cfg =>
            cfg.Ignore(destinationMember)
        );

        var memberName = ReflectionUtils.GetMemberName(destinationMember);
        konverter.RegisterIgnoredProperty((typeof(TSource), typeof(TDestination), memberName));

        return this;
    }

    public MapConfig<TSource, TDestination> When<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Func<TSource, bool> condition)
    {
        konverter.StoreConfig<TSource, TDestination>(cfg =>
            cfg.When(destinationMember, condition)
        );

        var memberName = ReflectionUtils.GetMemberName(destinationMember);
        konverter.RegisterConditionalMapping((typeof(TSource), typeof(TDestination), memberName),
            src => condition((TSource)src));

        return this;
    }

    public MapConfig<TSource, TDestination> BeforeMap(Action<TSource, TDestination> action)
    {
        konverter.StoreConfig<TSource, TDestination>(cfg =>
            cfg.BeforeMap(action)
        );

        konverter.RegisterBeforeMap(action);
        return this;
    }

    public MapConfig<TSource, TDestination> AfterMap(Action<TSource, TDestination> action)
    {
        konverter.StoreConfig<TSource, TDestination>(cfg =>
            cfg.AfterMap(action)
        );

        konverter.RegisterAfterMap(action);
        return this;
    }
}