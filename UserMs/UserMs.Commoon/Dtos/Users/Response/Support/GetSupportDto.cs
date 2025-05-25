using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Support
{
    public class GetSupportDto
    {
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
       // public string? UserPassword { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? UserPhone { get; set; } = string.Empty;
        public string? UserAddress { get; set; } = string.Empty;
        //public string? UserAvailable { get; init; }
        public string? UserLastName { get; set; } = string.Empty;
        public string SupportDni { get;  set; }
        public string SupportSpecialization { get;  set; }
        public bool SupportDelete { get; set; } = false;
    }
}
