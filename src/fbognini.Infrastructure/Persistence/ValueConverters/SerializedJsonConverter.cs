using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace fbognini.Infrastructure.Persistence.ValueConverters
{
    public class SerializedJsonConverter<T> : ValueConverter<T, string>
    {
        private static readonly JsonSerializerOptions options = new();

        public SerializedJsonConverter()
            : base(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<T>(v, options)!)
        {
        }
    }
}
