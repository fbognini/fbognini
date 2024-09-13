using System;
using System.Collections.Generic;
using System.Net;

namespace fbognini.Core.Exceptions;

public class IdentityException : AppException
{
    public IdentityException()
        : base(HttpStatusCode.Unauthorized)
    {
    }

    public IdentityException(string message, string? title = null)
        : base(HttpStatusCode.Unauthorized, message, title ?? "You're not authorized.")
    {
    }
}
