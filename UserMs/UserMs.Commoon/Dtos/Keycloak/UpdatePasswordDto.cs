using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Keycloak
{
    public class UpdatePasswordDto
    {
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
