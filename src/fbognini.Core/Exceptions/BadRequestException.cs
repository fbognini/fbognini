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

    public abstract class AdditionalData
    {
        public abstract string Entity { get; }
        public string Error { get; }

        protected AdditionalData(string error)
        {
            Error = error;
        }
    }

    public class BadRequestException: AppException
    {

        public BadRequestException()
            : base(HttpStatusCode.BadRequest)
        {

        }

        public BadRequestException(
            string message)
            : base(HttpStatusCode.BadRequest, message)
        {

        }

        public BadRequestException(
            object additionalData)
            : base(HttpStatusCode.BadRequest, additionalData)
        {

        }

        public BadRequestException(
            string message
            , object additionalData)
            : base(HttpStatusCode.BadRequest, message, additionalData)
        {
        }

        public BadRequestException(
            string message
            , Exception exception)
            : base(HttpStatusCode.BadRequest, message, exception)
        {
        }

        public BadRequestException(
            string message
            , Exception exception
            , object additionalData)
            : base(HttpStatusCode.BadRequest, message, exception, additionalData)
        {
        }
    }
}
