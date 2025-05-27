using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Application.Commands.Bidder
{
    public class UpdateBidderCommand : IRequest<GetBidderDto>
    {
        public UserId BidderId { get; set; }
        public UpdateBidderDto Bidder { get; set; }

        public UpdateBidderCommand(UserId bidderId, UpdateBidderDto bidder)
        {
            BidderId = bidderId;
            Bidder = bidder;
        }
    }
}
