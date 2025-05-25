using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;

namespace UserMs.Core.Repositories.Supports
{
    public interface ISupportRepositoryMongo
    {
        Task<Domain.Entities.Support.Supports?> GetSupportByIdAsync(UserId supportId);
        Task<Domain.Entities.Support.Supports?> GetSupportByEmailAsync(UserEmail supportUserId);
        Task<List<Domain.Entities.Support.Supports>> GetSupportAllAsync();
    }
}
