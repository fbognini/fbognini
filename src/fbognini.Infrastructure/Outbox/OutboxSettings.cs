using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace fbognini.Infrastructure.Outbox
{
    public class OutboxSettings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OutboxProcessorApplicationFilter ApplicationFilter { get; init; } = OutboxProcessorApplicationFilter.Me;
        public string? Applications { get; init; }
        public int IntervalInSeconds { get; init; } = -1;
        public int BatchSize { get; init; } = 100;
    }

    public enum OutboxProcessorApplicationFilter
    {
        None,
        Me,
        Custom,
        All
    }
}
