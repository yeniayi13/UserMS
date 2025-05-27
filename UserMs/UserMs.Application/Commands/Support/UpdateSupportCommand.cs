using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Support;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Application.Commands.Support
{
    public class UpdateSupportCommand : IRequest<GetSupportDto>
    {
        public UserId SupportId { get; set; }
        public UpdateSupportDto Support { get; set; }

        public UpdateSupportCommand(UserId supportId, UpdateSupportDto support)
        {
            SupportId = supportId;
            Support = support;
        }
    }
}
