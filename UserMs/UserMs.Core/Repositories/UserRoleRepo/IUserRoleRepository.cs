using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Core.Repositories.UserRoleRepo
{
    public interface IUserRoleRepository
    {
       
        Task AddAsync(UserRoles userRoles);
        Task<UserRoles?> UpdateUsersRoleAsync(UserId userId, UserRoles userRole);
        Task<UserRoles?> DeleteUsersRoleAsync(UserRoleId userRoleId);
    }
}
