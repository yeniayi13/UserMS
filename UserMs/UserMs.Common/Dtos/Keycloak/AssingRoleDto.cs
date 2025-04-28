using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthMs.Common.Dtos
{
    public class AssingRoleDto
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; } = String.Empty;
        public string ClientId { get; set; } = String.Empty;
    }
}
