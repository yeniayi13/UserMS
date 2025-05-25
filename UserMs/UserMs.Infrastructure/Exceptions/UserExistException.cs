using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMs.Common.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class UserExistException : Exception
    {
        public UserExistException() { }

        public UserExistException(string message)
            : base(message) { }

        public UserExistException(string message, Exception inner)
            : base(message, inner) { }
    }
}
