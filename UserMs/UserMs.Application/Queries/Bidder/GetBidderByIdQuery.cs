using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Bidder;

namespace UserMs.Application.Queries.Bidder
{
    public class GetBidderByIdQuery : IRequest<GetBidderDto>
    {
        public Guid BidderId { get; set; }

        public GetBidderByIdQuery(Guid bidderId)
        {
            BidderId = bidderId;
        }
    }
}
