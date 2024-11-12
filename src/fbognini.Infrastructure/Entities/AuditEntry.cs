using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FastIDs.TypeId.Serialization.SystemTextJson;
using System.Text.Json.Serialization;

namespace fbognini.Infrastructure.Entities
{
    public enum AuditType : byte
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3
    }

    public class AuditEntry
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        }.ConfigureForTypeId();

        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
            TableName = entry.Entity.GetType().Name;
        }

        public EntityEntry Entry { get; }
        public string? UserId { get; set; }
        public DateTime DateTime { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object?> KeyValues { get; } = new Dictionary<string, object?>();
        public Dictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
        public Dictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
        public AuditType AuditType { get; set; }
        public List<string> ChangedColumns { get; } = new List<string>();
        public bool HasTemporaryProperties => TemporaryProperties.Any();
        public bool HasProperties => OldValues.Any() || NewValues.Any();

        public Audit ToAudit()
        {
            var audit = new Audit
            {
                UserId = UserId,
                Type = AuditType.ToString(),
                TableName = TableName,
                DateTime = DateTime,
                PrimaryKey = JsonSerializer.Serialize(KeyValues, _jsonSerializerOptions),
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues, _jsonSerializerOptions),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues, _jsonSerializerOptions),
                AffectedColumns = ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns, _jsonSerializerOptions)
            };
            return audit;
        }
    }
}