
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Infrastructure.RabbitMQ.Consumer;

namespace UserMs.Infrastructure.RabbitMQ.Connection
{
    public class RabbitMQBackgroundService : BackgroundService
    {
        private readonly RabbitMQConsumer _rabbitMQConsumer;

        public RabbitMQBackgroundService(RabbitMQConsumer rabbitMQConsumer)
        {
            _rabbitMQConsumer = rabbitMQConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine(" Esperando la inicialización de RabbitMQ...");

            await Task.Delay(3000); // Pequeño retraso para asegurar la inicialización
            await _rabbitMQConsumer.ConsumeMessagesAsync("userQueue");

            Console.WriteLine(" Consumidor de RabbitMQ iniciado.");
        }
    }
}
