using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities;

namespace UserMs.Core.Repositories.Auctioneer
{
    public interface IAuctioneerRepositoryMongo
    {
        Task<Auctioneers?> GetAuctioneerByIdAsync(UserId auctioneerId);
        Task<Auctioneers?> GetAuctioneerByEmailAsync(UserEmail auctioneerUserId);
        Task<List<Auctioneers>> GetAuctioneerAllAsync();
    }
}
