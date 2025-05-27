using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Support;

namespace UserMs.Application.Queries.Support
{
    public class GetSupportByUserEmailQuery : IRequest<GetSupportDto>
    {
        public string Email { get; set; }

        public GetSupportByUserEmailQuery(string email)
        {
            Email = email;
        }
    }
}
