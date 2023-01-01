using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public class NotFoundAdditionalData : AdditionalData
    {
        public override string Entity { get; }
        public object Key { get; set; }

        public NotFoundAdditionalData(string entity, object key)
            : base("NotFound")
        {
            Entity = entity;
            Key = key;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException()
            : base(HttpStatusCode.NotFound)
        {

        }

        public NotFoundException(
            string message, string title = null)
            : base(HttpStatusCode.NotFound, message, title)
        {

        }

        public NotFoundException(
            Type type,
            object key)
            : base(
                  HttpStatusCode.NotFound,
                  $"Entity \"{type.Name}\" ({key}) was not found.",
                  $"{type.Name} was not found.",
                  new NotFoundAdditionalData(type.Name, key))
        {
        }
    }
}
