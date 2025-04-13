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

    public MapConfig<TDestination, TSource> ReverseMap() => konverter.ReverseMap<TSource, TDestination>();

    public MapConfig<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
        Func<TSource, Konverter, TMember> mapFunc)
    {
        var memberName = ReflectionUtils.GetMemberName(destinationMember);
        var key = (typeof(TSource), typeof(TDestination), memberName);

        konverter.RegisterCustomMapping(key, (Func<TSource, TMember>)(src => mapFunc(src, konverter)));

        return this;
    }
    public MapConfig<TSource, TDestination> BeforeMap(Action<TSource, TDestination> action)
    {
        konverter.RegisterBeforeMap(action);
        return this;
    }

    public MapConfig<TSource, TDestination> AfterMap(Action<TSource, TDestination> action)
    {
        konverter.RegisterAfterMap(action);
        return this;
    }
}