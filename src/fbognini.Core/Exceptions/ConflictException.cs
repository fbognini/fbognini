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

        public ConflictException(
            string message)
            : base(HttpStatusCode.Conflict, message)
        {

        }

        public ConflictException(
            object additionalData)
            : base(HttpStatusCode.Conflict, additionalData)
        {

        }

        public ConflictException(
            Type type
            , object key)
            : base(HttpStatusCode.Conflict, $"Entity \"{type}\" ({key}) was not found.", null)
        {
        }

        public ConflictException(
            string message
            , object additionalData)
            : base(HttpStatusCode.Conflict, message, additionalData)
        {
        }

        public ConflictException(
            string message
            , Exception exception)
            : base(HttpStatusCode.Conflict, message, exception)
        {
        }

        public ConflictException(
            string message
            , Exception exception
            , object additionalData)
            : base(HttpStatusCode.Conflict, message, exception, additionalData)
        {
        }
    }
}
