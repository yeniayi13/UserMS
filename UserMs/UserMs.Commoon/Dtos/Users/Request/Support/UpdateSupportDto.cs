using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Request.Support
{
    public class UpdateSupportDto
    {

        public string UserEmail { get; set; } = String.Empty;
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
       // public string UserPassword { get; private set; }
        public string SupportDni { get;  set; }
        public string SupportSpecialization { get;  set; }
        public bool SupportDelete { get; set; } = false;
    }
}
