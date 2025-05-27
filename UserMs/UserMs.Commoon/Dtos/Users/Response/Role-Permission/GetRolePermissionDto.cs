using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Role_Permission
{
    public class GetRolePermissionDto
    {

        [JsonIgnore]
        public Guid RolePermissionId { get; set; }
        [JsonIgnore]
        public Guid RoleId { get; set; }
        [JsonIgnore]
        public Guid PermissionId { get; set; }

        public string RoleName { get; set; } 
        public string PermissionName { get; set; }

       
    }
    
}
