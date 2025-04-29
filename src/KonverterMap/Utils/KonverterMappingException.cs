namespace KonverterMap.Utils
{
    public class KonverterMappingException : Exception
    {
        public KonverterMappingException(string message, Exception innerException)
        : base(message, innerException)
        {
        }
    }
}
