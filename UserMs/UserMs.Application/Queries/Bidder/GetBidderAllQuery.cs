using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Bidder;

namespace UserMs.Application.Queries.Bidder
{
    public class GetBidderAllQuery : IRequest<List<GetBidderDto>>
    {
        public Guid BidderId { get; private set; }
        public Guid UserId { get; private set; }
        public string BidderDni { get; private set; }
        public DateOnly BidderBirthday { get; private set; }
    }
}
