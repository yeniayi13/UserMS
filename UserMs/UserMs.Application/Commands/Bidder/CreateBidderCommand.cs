using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Application.Commands.Bidder
{
    public class CreateBidderCommand : IRequest<UserId>
    {
        public CreateBidderDto Bidder { get; set; }

        public CreateBidderCommand(CreateBidderDto bidder)
        {
            Bidder = bidder;
        }
    }
}
