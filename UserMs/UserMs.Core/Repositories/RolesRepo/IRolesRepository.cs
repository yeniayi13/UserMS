using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role;

namespace UserMs.Core.Repositories.RolesRepo
{
    public interface IRolesRepository
    {
        Task<List<Roles>> GetRolesAllQueryAsync();
        Task<Roles?> GetRolesByIdQuery(Guid userId);
        Task<Roles?> GetRolesByNameQuery(string roleName);
       
    }
}
