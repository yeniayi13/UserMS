using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;

namespace UserMs.Application.Queries.Auctioneer
{
    public class GetAuctioneerAllQuery : IRequest<List<GetAuctioneerDto>>
    {
        public Guid AuctioneerId { get; private set; }
        public Guid UserId { get; private set; }
        public string AuctioneerDni { get; private set; }
        public DateOnly AuctioneerBirthday { get; private set; }
    }
}
