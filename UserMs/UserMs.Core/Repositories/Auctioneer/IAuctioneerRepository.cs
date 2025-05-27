using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder;

namespace UserMs.Core.Repositories.Auctioneer
{
    public interface IAuctioneerRepository
    {
       
        Task AddAsync(Auctioneers auctioneer);
        Task<Auctioneers?> UpdateAsync(UserId auctioneerId, Auctioneers auctioneer);
        Task<Auctioneers?> DeleteAsync(UserId auctioneerId);
    }
}
