using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Core.Repositories.SupportsRepo
{
    public interface ISupportRepository
    {
       
        Task AddAsync(Domain.Entities.Support.Supports support);
        Task<Domain.Entities.Support.Supports?> UpdateAsync(UserId supportId, Domain.Entities.Support.Supports support);
        Task<Domain.Entities.Support.Supports?> DeleteAsync(UserId supportId);
    }
}
