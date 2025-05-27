using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMs.Common.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class KeycloakException : Exception
    {
        public KeycloakException() { }

        public KeycloakException(string message)
            : base(message) { }

        public KeycloakException(string message, Exception inner)
            : base(message, inner) { }
    }
}
