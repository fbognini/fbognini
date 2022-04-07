using fbognini.Core.Exceptions;
using System.Net;

namespace fbognini.Application.Multitenancy;

public class InvalidTenantException : AppException
{
    public InvalidTenantException(string message) : base(HttpStatusCode.BadRequest, message)
    {

    }
}