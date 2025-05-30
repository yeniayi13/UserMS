using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.UserRole;

namespace UserMs.Core.Repositories.UserRoleRepo
{
    public interface IUserRoleRepositoryMongo
    {
        Task<List<GetUserRoleDto>> GetUsersRoleAsync();
        Task<List<GetUserRoleDto>> GetUserRolesByUserEmailQuery(string email);
        Task<List<GetUserRoleDto>> GetUserRolesByRoleNameQuery(string name);
        Task<GetUserRoleDto?> GetRoleByRoleNameAndByUserEmail(string roleName, string userEmail);
        Task<List<GetUserRoleDto>> GetUserRolesByIdQuery(Guid userRoleId);
    }
}
