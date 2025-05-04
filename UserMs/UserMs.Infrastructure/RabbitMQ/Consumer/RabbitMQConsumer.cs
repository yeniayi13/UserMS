//using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using RabbitMQ.Client;
using UserMs.Domain.Entities.IUser.ValueObjects;
using System.Text.Json;
using UserMs.Infrastructure.JsonConverter.IUser;
using MongoDB.Driver;
using UserMs.Commoon.Dtos.Users.Request;

namespace UserMs.Infrastructure.RabbitMQ.Consumer
{
    public class RabbitMQConsumer
    {
        private readonly RabbitMQConnection _rabbitMQConnection;

        public RabbitMQConsumer(RabbitMQConnection rabbitMQConnection)
        {
            _rabbitMQConnection = rabbitMQConnection;
        }

        public async Task ConsumeMessagesAsync(string queueName)
        {
            var channel = _rabbitMQConnection.GetChannel();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensaje recibido: {message}"); // Verificación antes de procesarlo

                try
                {
                    var mongoClient = new MongoClient("mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
                    var database = mongoClient.GetDatabase("UserMs");
                    var collection = database.GetCollection<CreateUsersDto>("Users");

                    var record = JsonSerializer.Deserialize<CreateUsersDto>(message);

                    // 🔹 Insertar los datos **tal como los recibió** (sin Value Objects) en MongoDB
                    await collection.InsertOneAsync(record);

                    Console.WriteLine($"Mensaje insertado en MongoDB: {JsonSerializer.Serialize(record)}");

                    // 🔹 Confirmamos la recepción del mensaje
                    await Task.Run(() => channel.BasicAckAsync(ea.DeliveryTag, false));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            };

            // **IMPORTANTE: Aquí iniciamos la escucha de mensajes en la cola**
            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            Console.WriteLine("Consumidor de RabbitMQ escuchando mensajes...");
        }
    }
}