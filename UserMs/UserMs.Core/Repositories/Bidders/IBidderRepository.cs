using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Core.Repositories.Biddere
{
    public interface IBidderRepository
    {
      
        Task AddAsync(Domain.Entities.Bidder.Bidders bidder);
        Task<Domain.Entities.Bidder.Bidders?> UpdateAsync(UserId bidderId, Domain.Entities.Bidder.Bidders bidder);
        Task<Domain.Entities.Bidder.Bidders?> DeleteAsync(UserId bidderId);
    }
}
