using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder;

namespace UserMs.Core.Repositories
{
    public interface IBidderRepository
    {
        Task<List<Bidder>> GetBidderAsync();
        Task<Bidder?> GetBidderById(UserId userId);
        Task AddAsync(Bidder bidder);
        Task<Bidder?> UpdateBidderAsync(UserId userId, Bidder bidder);
        Task<Bidder?> DeleteBidderAsync(UserId userId);
    }
}
