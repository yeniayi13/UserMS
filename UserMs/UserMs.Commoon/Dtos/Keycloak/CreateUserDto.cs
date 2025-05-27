using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos
{

    [ExcludeFromCodeCoverage]
    public  class CreateUserDto
    {
        public string Email { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Phone { get; set; } = String.Empty;

        public string Address { get; set; } = String.Empty;

        public string Password { get; set; } = String.Empty;
    }
}
