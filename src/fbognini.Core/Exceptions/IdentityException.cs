using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public class IdentityException : AppException
    {
        public IdentityException()
            : base(HttpStatusCode.Unauthorized)
        {

        }

        public IdentityException(
            string message)
            : base(HttpStatusCode.Unauthorized, message)
        {

        }

        public IdentityException(
            object additionalData)
            : base(HttpStatusCode.Unauthorized, additionalData)
        {

        }

        public IdentityException(
            Type type
            , object key)
            : base(HttpStatusCode.Unauthorized, $"Entity \"{type}\" ({key}) was not found.", new NotFoundAdditionalData(type.Name, key))
        {
        }

        public IdentityException(
            string message
            , object additionalData)
            : base(HttpStatusCode.Unauthorized, message, additionalData)
        {
        }

        public IdentityException(
            string message
            , Exception exception)
            : base(HttpStatusCode.NotFound, message, exception)
        {
        }

        public IdentityException(
            string message
            , Exception exception
            , object additionalData)
            : base(HttpStatusCode.Unauthorized, message, exception, additionalData)
        {
        }
    }
}
