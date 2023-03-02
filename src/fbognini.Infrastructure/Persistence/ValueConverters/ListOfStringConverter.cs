using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Text.Json;

namespace fbognini.Infrastructure.Persistence.ValueConverters
{
    public class ListOfStringConverter : ValueConverter<List<string>, string>
    {
        private static readonly JsonSerializerOptions options = new();

        public ListOfStringConverter()
            : base(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<List<string>>(v, options))
        {
        }
    }
}
