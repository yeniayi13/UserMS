using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Domain.Entities;

namespace UserMs.Application.Commands.ActivityHistory
{
    public class CreateHistoryActivityCommand : IRequest<UserId>
    {
        public CreateActivityHistoryDto HistoryActivity { get; set; }

        public CreateHistoryActivityCommand(CreateActivityHistoryDto historyActivity)
        {
            HistoryActivity = historyActivity;
        }
    }
}
