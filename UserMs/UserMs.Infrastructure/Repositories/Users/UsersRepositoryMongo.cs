using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Database;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.Repositories.Users
{
    public class UsersRepositoryMongo : IUserRepositoryMongo
    {

        private readonly IMongoCollection<Domain.Entities.UserEntity.Users> _collection;
        private readonly IMapper _mapper;

        public UsersRepositoryMongo(IUserDbContextMongo context, IMapper mapper)
        {

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _collection = context.Database.GetCollection<Domain.Entities.UserEntity.Users>("Users");
            //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }

        public async Task<List<Domain.Entities.UserEntity.Users>> GetUsersAsync()
        {
            var projection = Builders<Domain.Entities.UserEntity.Users>.Projection.Exclude("_id"); // Excluir _id

            Console.WriteLine("Consulta en proceso...");

            var usersDto = await _collection
                .Find(Builders<Domain.Entities.UserEntity.Users>.Filter.Empty)
                .Project<GetUsersDto>(projection) //  Convierte los datos al DTO
                .ToListAsync()
                .ConfigureAwait(false);

            if (usersDto == null || usersDto.Count == 0)
            {
                Console.WriteLine("No se encontraron categorías.");
                return new List<Domain.Entities.UserEntity.Users>(); // Retorna una lista vacía en lugar de `null` para evitar errores
            }

            var usersEntity = _mapper.Map<List<Domain.Entities.UserEntity.Users>>(usersDto);

            return usersEntity;

        }

        public async Task<Domain.Entities.UserEntity.Users?> GetUsersById(Guid userId)
        {
            Console.WriteLine($"Buscando usuario con ID: {userId}");

            // Crear el filtro para buscar por UserId
            var filter = Builders<Domain.Entities.UserEntity.Users>.Filter.Eq("UserId", userId);

            // Excluir el campo _id de la consulta
            var projection = Builders<Domain.Entities.UserEntity.Users>.Projection
                .Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userDto = await _collection
                .Find(filter)
                .Project<GetUsersDto>(projection) // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            Console.WriteLine(userDto != null ? $"Usuario encontrado: {userDto}" : "Usuario no encontrado.");
            var usersEntity = _mapper.Map<Domain.Entities.UserEntity.Users>(userDto);


            return usersEntity;
        }


        public async Task<Domain.Entities.UserEntity.Users?> GetUsersByEmail(string userEmail)
        {
            Console.WriteLine($"Buscando usuario con ID: {userEmail}");

            // Crear el filtro para buscar por UserId
            var filter = Builders<Domain.Entities.UserEntity.Users>.Filter.Eq("UserEmail", userEmail);

            // Excluir el campo _id de la consulta
            var projection = Builders<Domain.Entities.UserEntity.Users>.Projection
                .Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userDto = await _collection
                .Find(filter)
                .Project<GetUsersDto>(projection) // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            Console.WriteLine(userDto != null ? $"Usuario encontrado: {userDto}" : "Usuario no encontrado.");
            var usersEntity = _mapper.Map<Domain.Entities.UserEntity.Users>(userDto);


            return usersEntity;
        }


        
    }

}


