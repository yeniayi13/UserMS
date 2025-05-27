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

namespace UserMs.Domain.Entities.Auctioneer
{
    public class Auctioneers: Base
    {
       
        public AuctioneerDni AuctioneerDni { get; private set; }
        
        public AuctioneerDelete AuctioneerDelete { get; private set; } //? no se si es necesario
        public AuctioneerBirthday AuctioneerBirthday { get; private set; } //? no se si es necesario


        public Auctioneers() { }

        public Auctioneers(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, AuctioneerDni auctioneerDni, AuctioneerBirthday auctioneerBirthday)
            : base(userId, userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
            AuctioneerDni = auctioneerDni;
            AuctioneerBirthday = auctioneerBirthday;
        }
      /*  public Auctioneers(UserId auctioneerUserId, AuctioneerDni auctioneerDni, AuctioneerBirthday auctioneerBirthday)
        {
           
            AuctioneerUserId = auctioneerUserId;
            AuctioneerDni = auctioneerDni;
            AuctioneerBirthday = auctioneerBirthday;
        }*/

        
        public void SetAuctioneerDni(AuctioneerDni auctioneerDni)
        {
            AuctioneerDni = auctioneerDni;
        }

        public void SetAuctioneerBirthday(AuctioneerBirthday auctioneerBirthday)
        {
            AuctioneerBirthday = auctioneerBirthday;
        }

        public void SetAuctioneerDelete(AuctioneerDelete auctioneerDelete)
        {
            AuctioneerDelete = auctioneerDelete;
        }

    }
}
