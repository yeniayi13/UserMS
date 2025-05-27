using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.Support.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Domain.Entities.Support
{
    public class Supports:Base
    {
        public SupportDni SupportDni { get; private set; }
        public SupportDelete SupportDelete { get; private set; } //? no se si es necesario
        public SupportSpecialization SupportSpecialization { get; private set; }
        

        public Supports() { }

        public Supports(UserId userId, UserEmail userEmail, UserPassword userPassword, UserName userName, UserPhone userPhone, UserAddress userAddress, UserLastName userLastName, SupportDni supportDni, SupportSpecialization supportSpecialization)
            : base(userId, userEmail, userPassword, userName, userPhone, userAddress, userLastName)
        {
            SupportDni = supportDni;
            SupportSpecialization = supportSpecialization;
        }

       /* public Supports( UserId supportUserId, SupportDni auctioneerDni, SupportSpecialization supportSpecialization)
        {
            
            SupportUserId = supportUserId;
            SupportDni = auctioneerDni;
            SupportSpecialization = supportSpecialization;
        }*/
        
        public void SetSupportDni(SupportDni supportDni)
        {
            SupportDni = supportDni;
        }

        public void SetSupportSpecialization(SupportSpecialization supportSpecialization)
        {
            SupportSpecialization = supportSpecialization;
        }

        public void SetSupportDelete(SupportDelete supportDelete)
        {
            SupportDelete = supportDelete;
        }

    }
}
