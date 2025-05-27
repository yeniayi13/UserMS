using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Domain.Entities.Bidder
{
    public class Bidders:Base
    {
        public BidderDni BidderDni { get; private set; }
        public BidderBirthday BidderBirthday { get; private set; }

        public BidderDelete BidderDelete { get; private set; }
        public Bidders() { }

        public Bidders(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, BidderDni bidderDni, BidderBirthday bidderBirthday)
            : base(userId, userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
            BidderDni = bidderDni;
            BidderBirthday = bidderBirthday;
        }

       /* public Bidders( UserId bidderUserId, BidderDni Bidder, BidderBirthday bidderBirthday)
        {
            
            BidderUserId = bidderUserId;
            BidderDni = Bidder;
            BidderBirthday = bidderBirthday;
        }*/

        public void SetBidderDni(BidderDni bidderDni)
        {
            BidderDni = bidderDni;
        }
        public void SetBidderBirthday(BidderBirthday bidderBirthday)
        {
            BidderBirthday = bidderBirthday;
        }
        public void SetBidderDelete(BidderDelete bidderDelete)
        {
            BidderDelete = bidderDelete;
        }

    }
}
