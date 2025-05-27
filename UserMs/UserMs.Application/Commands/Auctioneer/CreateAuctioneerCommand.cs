using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;

namespace UserMs.Application.Commands.Auctioneer
{
    public class CreateAuctioneerCommand : IRequest<UserId>
    {
        public CreateAuctioneerDto Auctioneer { get; set; }

        public CreateAuctioneerCommand(CreateAuctioneerDto auctioneer)
        {
            Auctioneer = auctioneer;
        }
    }
}
