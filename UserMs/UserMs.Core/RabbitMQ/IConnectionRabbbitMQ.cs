using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
namespace UserMs.Core.RabbitMQ
{
    public interface IConnectionRabbbitMQ
    {
        public Task InitializeAsync();
       public IChannel GetChannel();
    }
}
