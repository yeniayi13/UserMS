using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Core.Repositories.UserRepo
{
    public interface IUserRepositoryMongo
    {
        Task<List<Users>> GetUsersAsync();
        Task<Users?> GetUsersById(Guid userId);
        Task<Users?> GetUsersByEmail(string userEmail);
    }
}
