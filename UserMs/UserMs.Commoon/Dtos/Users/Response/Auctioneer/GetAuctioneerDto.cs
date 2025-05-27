using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Auctioneer
{
    public class GetAuctioneerDto
    {
       
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }

        [JsonIgnore]
        public string? UserPassword { get; set; }
        public string? AuctioneerDni { get;  set; }
        public string? UserName { get; set; } = string.Empty;
        public string? UserPhone { get; set; } = string.Empty;
        public string? UserAddress { get; set; } = string.Empty;
        [JsonIgnore]
        public string? UserAvailable {  get; init; }
        public string? UserLastName { get; set; } = string.Empty;
        public DateOnly AuctioneerBirthday { get;  set; }
        [JsonIgnore]
        public bool AuctioneerDelete {  get; set; } = false;
    }
}
