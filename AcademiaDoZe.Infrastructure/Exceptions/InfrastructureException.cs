using System;

namespace AcademiaDoZe.Infrastructure.Exceptions
{
    public class InfrastructureException : Exception
    {
        public string ErrorCode { get; }

        public InfrastructureException(string errorCode)
            : base()
        {
            ErrorCode = errorCode;
        }

        public InfrastructureException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public InfrastructureException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
