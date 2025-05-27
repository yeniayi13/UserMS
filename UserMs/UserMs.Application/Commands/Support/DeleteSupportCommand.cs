using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Application.Commands.Support
{
    public class DeleteSupportCommand : IRequest<UserId>
    {
        public UserId SupportId { get; set; }

        public DeleteSupportCommand(UserId supportId)
        {
            SupportId = supportId;
        }
    }
}
