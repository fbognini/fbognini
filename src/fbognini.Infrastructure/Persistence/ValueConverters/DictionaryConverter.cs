using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Text.Json;

namespace fbognini.Infrastructure.Persistence.ValueConverters
{
    public class DictionaryConverter : ValueConverter<Dictionary<string, object>, string>
    {
        private static readonly JsonSerializerOptions options = new();

        public DictionaryConverter()
            : base(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, options))
        {
        }
    }
}
