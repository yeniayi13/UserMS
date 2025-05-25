using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;

namespace UserMs.Core.Interface
{
  
    public interface IAuthMsService
    {
        Task<string> CreateUserAsync(string userEmail, string userPassword);
        Task<string> DeleteUserAsync(UserId userId);
        Task AssignClientRoleToUser(
            Guid userId,
            string roleName
        );
        Task<Guid> GetUserByUserName(UserEmail userName);
        Task UpdateUser(UserId id, Base user);
    }
}
