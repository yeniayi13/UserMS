using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;

namespace UserMs.Application.Commands.Auctioneer
{
    public class UpdateAuctioneerCommand : IRequest<GetAuctioneerDto>
    {
        public UserId AuctioneerId { get; set; }
        public UpdateAuctioneerDto Auctioneer { get; set; }

        public UpdateAuctioneerCommand(UserId auctioneerId, UpdateAuctioneerDto auctioneer)
        {
            AuctioneerId = auctioneerId;
            Auctioneer = auctioneer;
        }
    }
}
