using System;
using System.Net;

namespace fbognini.Core.Exceptions
{
    public abstract class AppException: Exception
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public object AdditionalData { get; set; }

        public AppException()
            : this(HttpStatusCode.InternalServerError)
        {
        }

        public AppException(
            HttpStatusCode httpStatusCode)
            : this(httpStatusCode, null)
        {
        }

        public AppException(
            string message,
            string title = null)
            : this(HttpStatusCode.InternalServerError, message, title)
        {
        }

        public AppException(
            HttpStatusCode httpStatusCode,
            string message,
            string title = null)
            : this(httpStatusCode, message, title, null)
        {
        }

        public AppException(
            HttpStatusCode httpStatusCode
            , object additionalData)
            : this(httpStatusCode, null, null, additionalData)
        {
        }

        public AppException(
            string message
            , Exception exception)
            : this(HttpStatusCode.InternalServerError, message, null, exception, null)
        {
        }

        public AppException(
           string message
           , object additionalData)
           : this(HttpStatusCode.InternalServerError, message, additionalData)
        {
        }

        public AppException(
            HttpStatusCode httpStatusCode
            , string message
            , object additionalData)
            : this(httpStatusCode, message, null, null, additionalData)
        {
        }

        public AppException(
            string message
            , Exception exception
            , object additionalData)
            : this(HttpStatusCode.InternalServerError, message, null, exception, additionalData)
        {
        }

        public AppException(
            HttpStatusCode httpStatusCode,
            string message,
            Exception exception)
            : this(httpStatusCode, message, null, exception, null)
        {
        }



        public AppException(
            HttpStatusCode httpStatusCode,
            string message,
            string title, 
            object additionalData)
            : this(httpStatusCode, message, title, null, additionalData)
        {
        }

        public AppException(
            HttpStatusCode httpStatusCode,
            string message,
            string title,
            Exception exception,
            object additionalData)
            : base(message, exception)
        {
            HttpStatusCode = httpStatusCode;
            Title = title;
            AdditionalData = additionalData;
        }
    }
}
