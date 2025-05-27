using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Domain.Entities.ActivityHistory
{
    public class ActivityHistory
    {
        public Guid Id { get; set; }
        public UserId UserId { get; set; }
        public string Action { get; set; }
        public Users User { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        public ActivityHistory(UserId userId, string action)
        {
            UserId = userId;
            Action = action;
        }
        public ActivityHistory(Guid id, UserId userId, string action)
        {
            Id = id;
            UserId = userId;
            Action = action;
        }
        public ActivityHistory(Guid id, UserId userId, string action, DateTime timestamp)
        {
            Id = id;
            UserId = userId;
            Action = action;
            Timestamp = timestamp;
        }
        public ActivityHistory()
        {
        }

    }
}
