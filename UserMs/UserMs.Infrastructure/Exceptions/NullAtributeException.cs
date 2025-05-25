using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class NullAtributeException : Exception
    {
        public NullAtributeException() { }

        public NullAtributeException(string message)
            : base(message) { }

        public NullAtributeException(string message, Exception inner)
            : base(message, inner) { }
    }
}