namespace KonverterMap.Helpers
{
    public static class Map
    {
        public static Func<TSource, Konverter, TDest> MapFrom<TSource, TOrigem, TDest>(
            Func<TSource, TOrigem> origemAccessor)
        {
            return (src, konverter) =>
            {
                var origem = origemAccessor(src);
                return origem == null ? default! : konverter.Map<TOrigem, TDest>(origem);
            };
        }
    }
}
