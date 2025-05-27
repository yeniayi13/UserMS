using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;

namespace UserMs.Application.Queries.Auctioneer
{
    public class GetAuctioneerByUserEmailQuery : IRequest<GetAuctioneerDto>
    {
        public string Email { get; set; }

        public GetAuctioneerByUserEmailQuery(string email)
        {
            Email = email;
        }
    }
}
