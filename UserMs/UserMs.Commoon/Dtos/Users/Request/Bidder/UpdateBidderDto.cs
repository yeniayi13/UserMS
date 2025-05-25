using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Request.Bidder
{
    public class UpdateBidderDto
    {

        public string UserEmail { get; set; } = String.Empty;
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
      //  public string UserPassword { get; set; }
        public string BidderDni { get;  set; }
        public DateOnly BidderBirthday { get;  set; }

        public bool BidderDelete { get; set; } = false;
    }
}
