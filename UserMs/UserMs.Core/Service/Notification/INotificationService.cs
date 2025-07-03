
using UserMs.Common.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimsMS.Core.Service.Notification
{
    public interface INotificationService
    {
        Task SendNotificationAsync(GetNotificationDto notification);
       
    }
}
