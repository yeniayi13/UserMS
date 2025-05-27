using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;

namespace UserMs.Core.Repositories.Bidders
{
    public interface IBidderRepositoryMongo
    {
        Task<Domain.Entities.Bidder.Bidders?> GetBidderByIdAsync(UserId bidderId);
        Task<Domain.Entities.Bidder.Bidders?> GetBidderByEmailAsync(UserEmail bidderUserEmail);
        Task<List<Domain.Entities.Bidder.Bidders>> GetBidderAllAsync();
    }
}
