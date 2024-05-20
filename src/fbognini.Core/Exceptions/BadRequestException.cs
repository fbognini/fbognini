using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public class BadRequestException : AppException
    {

        public BadRequestException()
            : base(HttpStatusCode.BadRequest)
        {
        }

        public BadRequestException(string? message, string? title = null)
            : base(HttpStatusCode.BadRequest, message, title)
        {
        }

        public BadRequestException(string message, object additionalData)
            : base(HttpStatusCode.BadRequest, message, additionalData)
        {
        }

        public BadRequestException(string message, Exception exception)
            : base(HttpStatusCode.BadRequest, message, exception)
        {
        }

        public BadRequestException(string message, Exception exception, object additionalData)
            : base(HttpStatusCode.BadRequest, message, null, exception, additionalData)
        {
        }
    }
}
