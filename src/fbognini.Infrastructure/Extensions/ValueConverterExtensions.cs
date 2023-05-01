using fbognini.Infrastructure.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace fbognini.Infrastructure.Extensions
{
    public static class ValueConverterExtensions
    {
        public static PropertyBuilder<T> HasSerializedJsonConversion<T>(this PropertyBuilder<T> builder)
        {
            return builder.HasConversion<SerializedJsonConverter<T>>();
        }
    }
}
