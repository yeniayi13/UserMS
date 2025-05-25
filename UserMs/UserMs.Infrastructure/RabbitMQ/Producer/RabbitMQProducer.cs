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

        public async Task PublishMessageAsync(T data, string queueName, string eventType)
        {
            var channel = _rabbitMQConnection.GetChannel();

            // Declaramos la cola de manera asincrónica
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            // 🔹 Envolver el mensaje con su tipo de evento
            var eventMessage = new
            {
                EventType = eventType, // "USER_CREATED" o "USER_UPDATED" o "USER_DELETED"
                Data = data
            };

            // 🔹 Serializar el mensaje en formato JSON
            var messageBody = Newtonsoft.Json.JsonConvert.SerializeObject(eventMessage);
            var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(messageBody));

            // 🔹 Crear propiedades básicas
            var basicProperties = new BasicProperties
            {
                ContentType = "application/json"
            };


            // 🔹 Publicar el mensaje con el tipo de evento
            await channel.BasicPublishAsync<BasicProperties>(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: basicProperties,
                body: body,
                cancellationToken: CancellationToken.None
            );

            Console.WriteLine($"Mensaje publicado en '{queueName}' con evento '{eventType}': {messageBody}");
        }
    }
}
