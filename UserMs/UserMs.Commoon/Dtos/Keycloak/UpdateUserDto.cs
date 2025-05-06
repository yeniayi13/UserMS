using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos
{
    public class UpdateUserDto
    {
        public string UserEmail { get; set; } = String.Empty;
        public string UserPassword { get; set; } = String.Empty;
        public string? UserName { get; set; } = String.Empty;
        public string? UserPhone { get; set; } = String.Empty;
        public string? UserAddress { get; set; } = String.Empty;
        public string? UserLastName { get; set; } = String.Empty;
    }
}
