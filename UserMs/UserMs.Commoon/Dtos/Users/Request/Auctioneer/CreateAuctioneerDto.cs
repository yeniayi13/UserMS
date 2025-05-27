using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Request.Auctioneer
{
    public class CreateAuctioneerDto
    {
        public string UserEmail { get; set; } = String.Empty;
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
        public string UserPassword { get; set; }
        //public Guid AuctioneerUserId { get;  set; }
        public string AuctioneerDni { get;  set; }
        public DateOnly AuctioneerBirthday { get;  set; }
        [JsonIgnore]
        public bool AuctioneerDelete { get; set; } = false;
    }
}
