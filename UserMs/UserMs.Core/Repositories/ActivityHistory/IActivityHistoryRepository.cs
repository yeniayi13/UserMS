using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.ActivityHistory;
using UserMs.Domain.Entities.Auctioneer;

namespace UserMs.Core.Repositories.ActivityHistoryRepo
{
    public interface IActivityHistoryRepository
    {
        Task AddAsync(Domain.Entities.ActivityHistory.ActivityHistory activity);
      
    }
}
