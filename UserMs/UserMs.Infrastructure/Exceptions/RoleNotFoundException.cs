using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Infrastructure.Exceptions
{
    public class RoleNotFoundException : Exception
    {
        public RoleNotFoundException() { }

        public RoleNotFoundException(string message)
            : base(message) { }

        public RoleNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
}
