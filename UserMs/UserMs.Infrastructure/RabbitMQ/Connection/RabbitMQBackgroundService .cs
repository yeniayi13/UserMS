
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Core.RabbitMQ;
using UserMs.Infrastructure.RabbitMQ.Consumer;

namespace UserMs.Infrastructure.RabbitMQ.Connection
{
    public class RabbitMQBackgroundService : BackgroundService
    {
        private readonly IRabbitMQConsumer _rabbitMQConsumer;

        public RabbitMQBackgroundService(IRabbitMQConsumer rabbitMQConsumer)
        {
            _rabbitMQConsumer = rabbitMQConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Esperando la inicialización de RabbitMQ...");

                await Task.Delay(3000); // Espera de 2 segundos antes de volver a comprobar
                

            var queues = new List<string> { "userQueue", "supportQueue", "bidderQueue", "auctioneerQueue", "userRoleQueue", "roleQueue", "rolePermissionQueue", "activityHistoryQueue" };

            foreach (var queueName in queues)
            {
                _ = Task.Run(() => _rabbitMQConsumer.ConsumeMessagesAsync(queueName), stoppingToken);
            }

            Console.WriteLine("🚀 Todos los consumidores de RabbitMQ han sido iniciados.");
        }
    }
}
