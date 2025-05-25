using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Permission;

namespace UserMs.Core.Repositories.PermissionRepo
{
    public interface IPermissionRepositoryMongo
    {
        Task<List<Permissions>> GetPermissionAllQueryAsync();
        Task<Permissions?> GetPermissionByIdAsync(Guid permissionId);
    }
}
