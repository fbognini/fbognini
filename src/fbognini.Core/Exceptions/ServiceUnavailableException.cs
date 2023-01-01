using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public class ServiceUnavailableException: AppException
    {

        public ServiceUnavailableException()
            : base(HttpStatusCode.ServiceUnavailable)
        {

        }

        public ServiceUnavailableException(
            string message)
            : base(HttpStatusCode.ServiceUnavailable, message)
        {

        }

        public ServiceUnavailableException(
            object additionalData)
            : base(HttpStatusCode.ServiceUnavailable, additionalData)
        {

        }

        public ServiceUnavailableException(
            string message
            , object additionalData)
            : base(HttpStatusCode.ServiceUnavailable, message, additionalData)
        {
        }

        public ServiceUnavailableException(
            string message
            , Exception exception)
            : base(HttpStatusCode.ServiceUnavailable, message, exception)
        {
        }

        public ServiceUnavailableException(
            string message
            , Exception exception
            , object additionalData)
            : base(HttpStatusCode.ServiceUnavailable, null, message, exception, additionalData)
        {
        }
    }
}
