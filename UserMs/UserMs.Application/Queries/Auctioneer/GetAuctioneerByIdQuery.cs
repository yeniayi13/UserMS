using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;

namespace UserMs.Application.Queries.Auctioneer
{
    public class GetAuctioneerByIdQuery : IRequest<GetAuctioneerDto>
    {
        public Guid AuctioneerId { get; set; }

        public GetAuctioneerByIdQuery(Guid auctioneerId)
        {
            AuctioneerId = auctioneerId;
        }
    }
}
