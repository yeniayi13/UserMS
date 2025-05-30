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
using Newtonsoft.Json;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.RabbitMQ;

namespace UserMs.Infrastructure.RabbitMQ.Consumer
{
    public class RabbitMQConsumer:IRabbitMQConsumer
    {
        private readonly IConnectionRabbbitMQ _rabbitMQConnection;
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<GetUsersDto> _collection;
        private readonly IMongoCollection<GetSupportDto> _collectionS;
        private readonly IMongoCollection<GetAuctioneerDto> _collectionA;

        private readonly IMongoCollection<GetBidderDto> _collectionB;
        private readonly IMongoCollection<GetUserRoleDto> _collectionUR;
        private readonly IMongoCollection<GetRolePermissionDto> _collectionPR;
        private readonly IMongoCollection<GetActivityHistoryDto> _collectionAH;

        /*private readonly IMongoCollection<UpdateUsersDto> _collectionU;
        private readonly IMongoCollection<GetUsersDto> _collectionG;*/


        private bool _isInitialized = false;

        public bool IsInitialized() => _isInitialized;
        public RabbitMQConsumer(IConnectionRabbbitMQ rabbitMQConnection)
        {
            _rabbitMQConnection = rabbitMQConnection;

            // 🔹 Conexión a MongoDB Atlas
            _mongoClient =
                new MongoClient(
                    "mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
            _database = _mongoClient.GetDatabase("UserMs");
            _collection = _database.GetCollection<GetUsersDto>("Users");
            _collectionS = _database.GetCollection<GetSupportDto>("Supports");
            _collectionA = _database.GetCollection<GetAuctioneerDto>("Auctioneers");
            _collectionB = _database.GetCollection<GetBidderDto>("Bidders");
            _collectionUR = _database.GetCollection<GetUserRoleDto>("UserRoles");
            _collectionPR = _database.GetCollection<GetRolePermissionDto>("RolePermissions");
            _collectionAH = _database.GetCollection<GetActivityHistoryDto>("ActivityHistories");
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
                Console.WriteLine($"Mensaje recibido en {queueName}: {message}");

                try
                {
                    Console.WriteLine($"Procesando evento en {queueName}...");

                    var baseEventMessage = JsonConvert.DeserializeObject<EventMessage<object>>(message);
                    if (baseEventMessage == null || string.IsNullOrEmpty(baseEventMessage.EventType))
                    {
                        Console.WriteLine("⚠ Evento no reconocido o mensaje mal formado.");
                        return;
                    }

                    // Procesar el mensaje según su cola
                    if (queueName == "userQueue" && baseEventMessage.EventType.StartsWith("USER_"))
                    {
                        var userEventMessage = JsonConvert.DeserializeObject<EventMessage<GetUsersDto>>(message);

                        await ProcessUserEvent(userEventMessage);
                    }
                    else if (queueName == "supportQueue" && baseEventMessage.EventType.StartsWith("SUPPORT_"))
                    {
                        var supportEventMessage = JsonConvert.DeserializeObject<EventMessage<GetSupportDto>>(message);
                        Console.WriteLine(
                            $"Datos antes de la inserción: {JsonConvert.SerializeObject(supportEventMessage.Data)}");
                        await ProcessSupportEvent(supportEventMessage);
                    }
                    else if (queueName == "bidderQueue" && baseEventMessage.EventType.StartsWith("BIDDER_"))
                    {
                        var bidderEventMessage = JsonConvert.DeserializeObject<EventMessage<GetBidderDto>>(message);
                        await ProcessBidderEvent(bidderEventMessage);
                    }
                    else if (queueName == "auctioneerQueue" && baseEventMessage.EventType.StartsWith("AUCTIONEER_"))
                    {
                        var auctioneerEventMessage =
                            JsonConvert.DeserializeObject<EventMessage<GetAuctioneerDto>>(message);
                        await ProcessAuctioneerEvent(auctioneerEventMessage);
                    }
                    else if (queueName == "userRoleQueue" && baseEventMessage.EventType.StartsWith("USER_ROLE_"))
                    {
                        Console.WriteLine(
                            $"Condumido:");
                        var userRoleEventMessage =
                            JsonConvert.DeserializeObject<EventMessage<GetUserRoleDto>>(message);
                        await ProcessUserRoleEvent(userRoleEventMessage);
                    }
                    else if (queueName == "rolePermissionQueue" && baseEventMessage.EventType.StartsWith("ROLE_PERMISSION_"))
                    {
                        Console.WriteLine(
                            $"Condumido:");
                        var rolePermissionEventMessage =
                            JsonConvert.DeserializeObject<EventMessage<GetRolePermissionDto>>(message);
                        await ProcessRolePermissionEvent(rolePermissionEventMessage);
                    }
                    else if (queueName == "activityHistoryQueue" && baseEventMessage.EventType.StartsWith("ACTIVITY_"))
                    {
                        Console.WriteLine(
                            $"Condumido:");
                        var activityEventMessage =
                            JsonConvert.DeserializeObject<EventMessage<GetActivityHistoryDto>>(message);
                        await ProcessActivityHistoryEvent(activityEventMessage);
                    }
                    else
                    {
                        Console.WriteLine($"⚠ Evento no reconocido en {queueName}: {baseEventMessage.EventType}");
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando el mensaje en {queueName}: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            Console.WriteLine($"Consumidor escuchando mensajes en {queueName}...");
        }


        private async Task ProcessActivityHistoryEvent(EventMessage<GetActivityHistoryDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {
                case "ACTIVITY_CREATED":
                    await _collectionAH.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine($"Actividad registrada en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;
            }
        }
        private async Task ProcessRolePermissionEvent(EventMessage<GetRolePermissionDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {
                case "ROLE_PERMISSION_CREATED":
                    await _collectionPR.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine($"Permiso de rol insertado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "ROLE_PERMISSION_UPDATED":
                    var filterRolePermission = Builders<GetRolePermissionDto>.Filter.Eq("RolePermissionId", eventMessage.Data.RolePermissionId);
                    var updateRolePermission = Builders<GetRolePermissionDto>.Update
                        .Set(rp => rp.RoleId, eventMessage.Data.RoleId)
                        .Set(rp => rp.PermissionId, eventMessage.Data.PermissionId)
                        .Set(rp => rp.RoleName, eventMessage.Data.RoleName)
                        .Set(rp => rp.PermissionName, eventMessage.Data.PermissionName);

                    await _collectionPR.UpdateOneAsync(filterRolePermission, updateRolePermission);
                    Console.WriteLine($"Permiso de rol actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "ROLE_PERMISSION_DELETED":
                    var deleteRolePermissionFilter = Builders<GetRolePermissionDto>.Filter.Eq("RolePermissionId", eventMessage.Data.RolePermissionId);
                    await _collectionPR.DeleteOneAsync(deleteRolePermissionFilter);
                    Console.WriteLine($"Permiso de rol eliminado en MongoDB con ID: {eventMessage.Data.RolePermissionId}");
                    break;
            }
        }
        private async Task ProcessUserEvent(EventMessage<GetUsersDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {
                case "USER_CREATED":

                    await _collection.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine(
                        $"Usuario insertado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "USER_UPDATED":
                    var filterUser = Builders<GetUsersDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    var updateUser = Builders<GetUsersDto>.Update
                        .Set(u => u.UserEmail, eventMessage.Data.UserEmail)
                        //.Set(u => u.UserPassword, eventMessage.Data.UserPassword)
                        .Set(u => u.UserName, eventMessage.Data.UserName)
                        .Set(u => u.UserPhone, eventMessage.Data.UserPhone)
                        .Set(u => u.UserAddress, eventMessage.Data.UserAddress)
                        .Set(u => u.UserLastName, eventMessage.Data.UserLastName)
                        .Set(u => u.UsersType, eventMessage.Data.UsersType)
                        .Set(u => u.UserAvailable, eventMessage.Data.UserAvailable);


                    await _collection.UpdateOneAsync(filterUser, updateUser);
                    Console.WriteLine(
                        $"Usuario actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "USER_DELETED":
                    var deleteUserFilter = Builders<GetUsersDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    await _collection.DeleteOneAsync(deleteUserFilter);
                    Console.WriteLine($"Usuario eliminado en MongoDB con ID: {eventMessage.Data.UserId}");
                    break;
            }
        }

        private async Task ProcessUserRoleEvent(EventMessage<GetUserRoleDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {
                case "USER_ROLE_CREATED":
                    await _collectionUR.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine(
                        $"Rol de usuario insertado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "USER_ROLE_UPDATED":
                    var filterUserRole = Builders<GetUserRoleDto>.Filter.Eq("UserRoleId", eventMessage.Data.UserRoleId);
                    var updateUserRole = Builders<GetUserRoleDto>.Update
                        //.Set(ur => ur.User.UserId, eventMessage.Data.User.UserId)
                        .Set(ur => ur.UserEmail, eventMessage.Data.UserEmail)
                        .Set(ur => ur.RoleName, eventMessage.Data.RoleName);
                        //.Set(ur => ur.IsDeleted, eventMessage.Data.IsDeleted);

                    await _collectionUR.UpdateOneAsync(filterUserRole, updateUserRole);
                    Console.WriteLine(
                        $"Rol de usuario actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "USER_ROLE_DELETED":
                    var deleteUserRoleFilter = Builders<GetUserRoleDto>.Filter.Eq("UserRoleId", eventMessage.Data.UserRoleId);
                    await _collectionUR.DeleteOneAsync(deleteUserRoleFilter);
                    Console.WriteLine($"Rol de usuario eliminado en MongoDB con ID: {eventMessage.Data.UserRoleId}");
                    break;
            }
        }
        private async Task ProcessAuctioneerEvent(EventMessage<GetAuctioneerDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {
                case "AUCTIONEER_CREATED":
                    await _collectionA.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine(
                        $"Auctioneer insertado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "AUCTIONEER_UPDATED":
                    var filterAuctioneer =
                        Builders<GetAuctioneerDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    var updateAuctioneer = Builders<GetAuctioneerDto>.Update
                        .Set(u => u.UserEmail, eventMessage.Data.UserEmail)
                       // .Set(u => u.UserPassword, eventMessage.Data.UserPassword)
                        .Set(u => u.UserName, eventMessage.Data.UserName)
                        .Set(u => u.UserPhone, eventMessage.Data.UserPhone)
                        .Set(u => u.UserAddress, eventMessage.Data.UserAddress)
                        .Set(u => u.UserLastName, eventMessage.Data.UserLastName)
                        .Set(a => a.AuctioneerDni, eventMessage.Data.AuctioneerDni)
                        .Set(a => a.AuctioneerBirthday, eventMessage.Data.AuctioneerBirthday);

                    await _collectionA.UpdateOneAsync(filterAuctioneer, updateAuctioneer);
                    Console.WriteLine(
                        $"Auctioneer actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "AUCTIONEER_DELETED":
                    var deleteAuctioneerFilter =
                        Builders<GetAuctioneerDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    await _collectionA.DeleteOneAsync(deleteAuctioneerFilter);
                    Console.WriteLine($"Auctioneer eliminado en MongoDB con ID: {eventMessage.Data.UserId}");
                    break;
            }
        }

        private async Task ProcessBidderEvent(EventMessage<GetBidderDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {
                case "BIDDER_CREATED":
                    await _collectionB.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine($"Bidder insertado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "BIDDER_UPDATED":
                    var filterBidder = Builders<GetBidderDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    var updateBidder = Builders<GetBidderDto>.Update
                        .Set(u => u.UserEmail, eventMessage.Data.UserEmail)
                        //.Set(u => u.UserPassword, eventMessage.Data.UserPassword)
                        .Set(u => u.UserName, eventMessage.Data.UserName)
                        .Set(u => u.UserPhone, eventMessage.Data.UserPhone)
                        .Set(u => u.UserAddress, eventMessage.Data.UserAddress)
                        .Set(u => u.UserLastName, eventMessage.Data.UserLastName)
                        .Set(b => b.BidderDni, eventMessage.Data.BidderDni)
                        .Set(b => b.BidderBirthday, eventMessage.Data.BidderBirthday);

                    await _collectionB.UpdateOneAsync(filterBidder, updateBidder);
                    Console.WriteLine(
                        $"Bidder actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "BIDDER_DELETED":
                    var deleteBidderFilter = Builders<GetBidderDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    await _collectionB.DeleteOneAsync(deleteBidderFilter);
                    Console.WriteLine($"Bidder eliminado en MongoDB con ID: {eventMessage.Data.UserId}");
                    break;
            }
        }

        private async Task ProcessSupportEvent(EventMessage<GetSupportDto> eventMessage)
        {
            if (eventMessage == null || eventMessage.Data == null) return;

            switch (eventMessage.EventType)
            {

                case "SUPPORT_CREATED":
                    Console.WriteLine($"Datos antes de la inserción: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    await _collectionS.InsertOneAsync(eventMessage.Data);
                    Console.WriteLine(
                        $"Support insertado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "SUPPORT_UPDATED":
                    var filterSupport = Builders<GetSupportDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    var updateSupport = Builders<GetSupportDto>.Update
                        .Set(u => u.UserEmail, eventMessage.Data.UserEmail)
                        //.Set(u => u.UserPassword, eventMessage.Data.UserPassword)
                        .Set(u => u.UserName, eventMessage.Data.UserName)
                        .Set(u => u.UserPhone, eventMessage.Data.UserPhone)
                        .Set(u => u.UserAddress, eventMessage.Data.UserAddress)
                        .Set(u => u.UserLastName, eventMessage.Data.UserLastName)
                        .Set(s => s.SupportDni, eventMessage.Data.SupportDni)
                        .Set(s => s.SupportSpecialization, eventMessage.Data.SupportSpecialization);

                    await _collectionS.UpdateOneAsync(filterSupport, updateSupport);
                    Console.WriteLine(
                        $"Support actualizado en MongoDB: {JsonConvert.SerializeObject(eventMessage.Data)}");
                    break;

                case "SUPPORT_DELETED":
                    var deleteSupportFilter = Builders<GetSupportDto>.Filter.Eq("UserId", eventMessage.Data.UserId);
                    await _collectionS.DeleteOneAsync(deleteSupportFilter);
                    Console.WriteLine($"Support eliminado en MongoDB con ID: {eventMessage.Data.UserId}");
                    break;
            }
        }
    }
}