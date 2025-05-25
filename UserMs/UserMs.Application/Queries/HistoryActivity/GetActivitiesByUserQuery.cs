using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Domain.Entities;

namespace UserMs.Application.Queries.HistoryActivity
{
    public class GetActivitiesByUserQuery : IRequest<List<GetActivityHistoryDto>>
    {
        public UserId UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public GetActivitiesByUserQuery(UserId userId, DateTime? startDate, DateTime? endDate)
        {
            UserId = userId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
