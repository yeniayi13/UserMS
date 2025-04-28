using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Domain.Entities.Bidder
{
    public class Bidder
    {
        public BidderId BidderId { get; private set; }
        public BidderUserId UserId { get; private set; }
        public BidderDni BidderDni { get; private set; }

        public Bidder() { }

        public Bidder(BidderId bidderId, BidderUserId userId, BidderDni Bidder)
        {
            BidderId = bidderId;
            UserId = userId;
            BidderDni = Bidder;
        }

        public void SetAuctioneerId(BidderId bidderId)
        {
            BidderId = bidderId;
        }
        public void SetUserId(BidderUserId userId)
        {
            UserId = userId;
        }
        public void SetAuctioneerDni(BidderDni bidderDni)
        {
            BidderDni = bidderDni;
        }
    }
}
