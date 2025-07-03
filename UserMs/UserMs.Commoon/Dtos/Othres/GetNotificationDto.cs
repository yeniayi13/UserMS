using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Common.Dtos.Response
{
    public class GetNotificationDto
    {
        public Guid NotificationId { get; set; }
        public string NotificationMessage { get; set; }
        public string NotificationSubject { get; set; }
        public Guid NotificationUserId { get; set; }
        public DateTime NotificationDateTime { get; set; }

        public string NotificationStatus { get; set; }
    }
}
