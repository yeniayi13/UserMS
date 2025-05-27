using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;

namespace UserMs.Application.Commands.Auctioneer
{
    public class DeleteAuctioneerCommand : IRequest<UserId>
    {
        public UserId AuctioneerId { get; set; }

        public DeleteAuctioneerCommand(UserId auctioneerId)
        {
            AuctioneerId = auctioneerId;
        }
    }
}
