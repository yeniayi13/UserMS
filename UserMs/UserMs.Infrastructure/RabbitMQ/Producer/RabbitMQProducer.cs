using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UserMs.Core.RabbitMQ;
namespace UserMs.Infrastructure.RabbitMQ
{


    public class RabbitMQProducer<T> : IEventBus<T>
    {
        private readonly RabbitMQConnection _rabbitMQConnection;

        public RabbitMQProducer(RabbitMQConnection rabbitMQConnection)
        {
            _rabbitMQConnection = rabbitMQConnection;
        }

        public async Task PublishMessageAsync(T message, string queueName)
        {
            var channel = _rabbitMQConnection.GetChannel();

            // Declaramos la cola de manera asincrónica
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            // Serializamos el mensaje y lo convertimos en un ReadOnlyMemory<byte>
            var messageBody = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(messageBody));

            // Creamos las propiedades básicas
            var basicProperties = new BasicProperties
            {
                ContentType = "application/json"
            };

            // Publicamos el mensaje especificando explícitamente el tipo para TProperties
            await channel.BasicPublishAsync<BasicProperties>(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: basicProperties,
                body: body,
                cancellationToken: CancellationToken.None
            );

            Console.WriteLine($"Mensaje publicado en la cola '{queueName}': {messageBody}");
        }


    }
}
