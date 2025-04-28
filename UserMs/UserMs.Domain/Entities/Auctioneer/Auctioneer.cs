using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;

namespace UserMs.Domain.Entities.Auctioneer
{
    public class Auctioneer
    {
        public AuctioneerId AuctioneerId { get; private set; }
        public AuctioneerUserId UserId { get; private set; }
        public AuctioneerDni AuctioneerDni { get; private set; }

        public Auctioneer() { }

        public Auctioneer(AuctioneerId auctioneerId, AuctioneerUserId userId, AuctioneerDni auctioneerDni)
        {
            AuctioneerId = auctioneerId;
            UserId = userId;
            AuctioneerDni = auctioneerDni;
        }

        public void SetAuctioneerId(AuctioneerId auctioneerId)
        {
            AuctioneerId = auctioneerId;
        }
        public void SetUserId(AuctioneerUserId userId)
        {
            UserId = userId;
        }
        public void SetAuctioneerDni(AuctioneerDni auctioneerDni)
        {
            AuctioneerDni = auctioneerDni;
        }

    }
}
