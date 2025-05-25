using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Bidder;

namespace UserMs.Application.Queries.Bidder
{
    public class GetBidderByUserEmailQuery : IRequest<GetBidderDto>
    {
        public string Email { get; set; }

        public GetBidderByUserEmailQuery(string email)
        {
            Email = email;
        }
    }
}
