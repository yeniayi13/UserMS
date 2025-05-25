using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;

namespace UserMs.Core.Repositories.ActivityHistory
{
    public interface IActivityHistoryRepositoryMongo
    {
        Task<List<Domain.Entities.ActivityHistory.ActivityHistory>> GetActivitiesByUserAsync(UserId userId, DateTime? startDate, DateTime? endDate);
    }
}
