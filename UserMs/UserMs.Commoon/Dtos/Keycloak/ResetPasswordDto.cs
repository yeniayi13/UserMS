using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Keycloak
{

    [ExcludeFromCodeCoverage]
    public class ResetPasswordDto
    {
        public string UserEmail { get; set; }

    }
}
