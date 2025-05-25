using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Bidder
{
    public class GetBidderDto
    {
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
       // public string? UserPassword { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? UserPhone { get; set; } = string.Empty;
        public string? UserAddress { get; set; } = string.Empty;
        //public string? UserAvailable { get; init; }
        public string? UserLastName { get; set; } = string.Empty;
        public string BidderDni { get;  set; }
        public DateOnly BidderBirthday { get;  set; }
       //public bool BidderDelete { get; set; } = false;
    }

}
