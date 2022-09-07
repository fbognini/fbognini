namespace fbognini.Infrastructure.Multitenancy
{
    public class DatabaseSettings
    {
        public string DBProvider { get; set; }
        public string ConnectionString { get; set; }
        public bool UseFakeMultitenancy { get; set; }
    }
}
