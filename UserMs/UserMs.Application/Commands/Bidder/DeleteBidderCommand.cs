using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Application.Commands.Bidder
{
    public class DeleteBidderCommand : IRequest<UserId>
    {
        public UserId BidderId { get; set; }

        public DeleteBidderCommand(UserId bidderId)
        {
            BidderId = bidderId;
        }
    }
}
