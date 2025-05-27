using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;

namespace UserMs.Commoon.Dtos.Users.Request.ActivityHistory
{
    public class CreateActivityHistoryDto
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
