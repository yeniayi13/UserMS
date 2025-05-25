using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ValidatorException : Exception
    {
        public ValidatorException()
        {
        }

        public ValidatorException(string message)
            : base(message)
        {
        }

        public ValidatorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}