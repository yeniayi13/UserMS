using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;


namespace UserMs.Core.Repositories.UserRepo

{
    public interface IUserRepository
    {
       
        Task AddAsync(Users users);
        Task<Users?> UpdateUsersAsync(UserId userId, Users user);
        Task<Users?> DeleteUsersAsync(UserId userId);
    }
}
