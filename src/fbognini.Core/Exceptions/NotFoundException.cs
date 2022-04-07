using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException()
            : base(HttpStatusCode.NotFound)
        {

        }

        public NotFoundException(
            string message)
            : base(HttpStatusCode.NotFound, message)
        {

        }

        public NotFoundException(
            object additionalData)
            : base(HttpStatusCode.NotFound, additionalData)
        {

        }

        public NotFoundException(
            Type type
            , object key)
            : base(HttpStatusCode.NotFound, $"Entity \"{type}\" ({key}) was not found.", new NotFoundAdditionalData(type.Name, key))
        {
        }

        public NotFoundException(
            string message
            , object additionalData)
            : base(HttpStatusCode.NotFound, message, additionalData)
        {
        }

        public NotFoundException(
            string message
            , Exception exception)
            : base(HttpStatusCode.NotFound, message, exception)
        {
        }

        public NotFoundException(
            string message
            , Exception exception
            , object additionalData)
            : base(HttpStatusCode.NotFound, message, exception, additionalData)
        {
        }
    }
}
