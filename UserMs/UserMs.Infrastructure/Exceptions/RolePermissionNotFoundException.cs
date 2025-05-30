using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Infrastructure.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class RolePermissionNotFoundException : Exception
    {
        public RolePermissionNotFoundException() { }

        public RolePermissionNotFoundException(string message)
            : base(message) { }

        public RolePermissionNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
}
