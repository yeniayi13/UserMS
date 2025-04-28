using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Bidder;

namespace UserMs.Core.Repositories
{
    public interface IAuctioneerRepository
    {
        Task<List<Auctioneer>> GetAuctioneerAsync();
        Task<Auctioneer?> GetAuctioneerById(UserId userId);
        Task AddAsync(Auctioneer auctioneer);
        Task<Auctioneer?> UpdateAuctioneerAsync(UserId userId, Auctioneer bidder);
        Task<Auctioneer?> DeleteAuctioneerAsync(UserId userId);
    }
}
