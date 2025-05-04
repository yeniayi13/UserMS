using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos
{
    public  class CreateUserDto
    {
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
    }
}
