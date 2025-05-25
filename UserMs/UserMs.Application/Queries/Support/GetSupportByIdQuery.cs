using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using UserMs.Commoon.Dtos.Users.Response.Support;

namespace UserMs.Application.Queries.Support
{
    public class GetSupportByIdQuery : IRequest<GetSupportDto>
    {
        public Guid SupportId { get; set; }

        public GetSupportByIdQuery(Guid supportId)
        {
            SupportId = supportId;
        }
    }
}
