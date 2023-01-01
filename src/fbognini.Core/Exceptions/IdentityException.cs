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
            string message, string title = null)
            : base(HttpStatusCode.Unauthorized, message, title ?? "You're not authorized.")
        {

        }

        public IdentityException(
            object additionalData)
            : base(HttpStatusCode.Unauthorized, additionalData)
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
            : base(HttpStatusCode.Unauthorized, null, message, exception, additionalData)
        {
        }
    }
}
