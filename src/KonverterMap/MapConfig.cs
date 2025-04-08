using KonverterMap;
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
}