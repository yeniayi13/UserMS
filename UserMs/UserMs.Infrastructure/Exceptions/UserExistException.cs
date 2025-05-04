using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMs.Common.Exceptions
{
    public class UserExistException : Exception
    {
        public UserExistException() { }

        public UserExistException(string message)
            : base(message) { }

        public UserExistException(string message, Exception inner)
            : base(message, inner) { }
    }
}
