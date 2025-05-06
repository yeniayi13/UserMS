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
using Newtonsoft.Json;
using UserMs.Application.Dtos.Users.Response;

namespace UserMs.Infrastructure.RabbitMQ.Consumer
{
    public class RabbitMQConsumer
    {
        private readonly RabbitMQConnection _rabbitMQConnection;
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<CreateUsersDto> _collection;
        private readonly IMongoCollection<UpdateUsersDto> _collectionU;
        private readonly IMongoCollection<GetUsersDto> _collectionG;
        public RabbitMQConsumer(RabbitMQConnection rabbitMQConnection)
        {
            _rabbitMQConnection = rabbitMQConnection;

            // 🔹 Conexión a MongoDB Atlas
            _mongoClient = new MongoClient("mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
            _database = _mongoClient.GetDatabase("UserMs");
            _collection = _database.GetCollection<CreateUsersDto>("Users");
            _collectionU = _database.GetCollection<UpdateUsersDto>("Users");
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
                Console.WriteLine($"Mensaje recibido: {message}");

                try
                {
                    var eventMessageC = JsonConvert.DeserializeObject<EventMessage<CreateUsersDto>>(message);
                    var eventMessageU = JsonConvert.DeserializeObject<EventMessage<UpdateUsersDto>>(message);
                    var eventMessageD = JsonConvert.DeserializeObject<EventMessage<GetUsersDto>>(message);
                    if (eventMessageC?.EventType == "USER_CREATED")
                    {
                        await _collection.InsertOneAsync(eventMessageC.Data);
                        Console.WriteLine($"Usuario insertado en MongoDB: {JsonConvert.SerializeObject(eventMessageC.Data)}");
                    }
                    else if (eventMessageU?.EventType == "USER_UPDATED")
                    {
                        var filter = Builders<UpdateUsersDto>.Filter.Eq("UserId", eventMessageU.Data.UserId);
                        var update = Builders<UpdateUsersDto>.Update
                            .Set(u => u.UserEmail, eventMessageU.Data.UserEmail)
                            .Set(u => u.UserName, eventMessageU.Data.UserName)
                            .Set(u => u.UserPhone, eventMessageU.Data.UserPhone)
                            .Set(u => u.UserAddress, eventMessageU.Data.UserAddress)
                            .Set(u => u.UserLastName, eventMessageU.Data.UserLastName)
                            .Set(u => u.UsersType, eventMessageU.Data.UsersType)
                            .Set(u => u.UserAvailable, eventMessageU.Data.UserAvailable);

                        await _collectionU.UpdateOneAsync(filter, update);
                        Console.WriteLine($"Usuario actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessageU.Data)}");
                    }
                    else if (eventMessageD?.EventType == "USER_DELETED")
                    {
                        var filter = Builders<UpdateUsersDto>.Filter.Eq("UserId", eventMessageD.Data.UserId);
                        await _collectionU.DeleteOneAsync(filter);
                        Console.WriteLine($"Usuario eliminado en MongoDB con ID: {eventMessageD.Data.UserId}");
                    }


                    await Task.Run(() => channel.BasicAckAsync(ea.DeliveryTag, false));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Consumidor de RabbitMQ escuchando mensajes...");
        }
    }
}