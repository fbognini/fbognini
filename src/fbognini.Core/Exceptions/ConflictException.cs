using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public class ConflictException : AppException
    {

        public ConflictException()
            : base(HttpStatusCode.Conflict)
        {
        }

        public ConflictException(string? message, string? title = null)
            : base(HttpStatusCode.Conflict, message, title)
        {
        }

        public ConflictException(string message, object additionalData)
            : base(HttpStatusCode.Conflict, message, additionalData)
        {
        }

        public ConflictException(string message, Exception exception)
            : base(HttpStatusCode.Conflict, message, exception)
        {
        }

        public ConflictException(string message, Exception exception, object additionalData)
            : base(HttpStatusCode.Conflict, message, null, exception, additionalData)
        {
        }
    }
}
