using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Domain.Entities;

namespace UserMs.Core.Repositories
{
    public interface IUserRepository
    {
        Task<List<Users>> GetUsersAsync();
        Task<Users?> GetUsersById(Guid userId);
        Task AddAsync(Users users);
        Task<Users?> UpdateUsersAsync(UserId userId, Users users);
        Task<Users?> DeleteUsersAsync(UserId userId);
    }
}
