using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos
{

    [ExcludeFromCodeCoverage]
    public class TokenDto
    {
        public string RefreshToken { get; set; } = String.Empty;
        public string AuthToken { get; set; } = String.Empty;
    }
}
