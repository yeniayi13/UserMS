using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Infrastructure.Exceptions
{
    public class UserRoleExistException : Exception
    {
        public UserRoleExistException() { }

        public UserRoleExistException(string message)
            : base(message) { }

        public UserRoleExistException(string message, Exception inner)
            : base(message, inner) { }
    }
}
