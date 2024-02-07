using System;

namespace fbognini.Core.Exceptions
{
    public sealed class SilentException: Exception, ISilentException
    {
        public SilentException() { }
        public SilentException(string message) : base(message) { }
        public SilentException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// If an exception implement ISilentException it won't be logged as an error by middleware
    /// </summary>
    public interface ISilentException
    {
    }
}

