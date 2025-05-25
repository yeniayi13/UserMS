using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Database;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Infrastructure.Repositories.User_Roles
{
    public class UserRolesRepositoryMongo : IUserRoleRepositoryMongo
    {

    
        private readonly IMongoCollection<GetUserRoleDto> _collection;
        private readonly IMapper _mapper;
        public UserRolesRepositoryMongo( IUserDbContextMongo context, IMapper mapper)
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
            _collection = context.Database.GetCollection<GetUserRoleDto>("UserRoles");
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }
        

        public async Task<List<GetUserRoleDto>> GetUserRolesByIdQuery(Guid userRoleId)
        {

            Console.WriteLine($"Buscando roles del usuario con ID: {userRoleId}");

            // Crear el filtro para buscar por UserId
            var filter = Builders<GetUserRoleDto>.Filter.Eq("_id", userRoleId);

            // Excluir el campo _id de la consulta
            var projection = Builders<GetUserRoleDto>.Projection.Include("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userRolesDto = await _collection
                .Find(filter)
                .Project<GetUserRoleDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(userRolesDto != null ? $"Roles encontrados: {userRolesDto}" : "No se encontraron roles para el usuario.");

            var userRolesDtoList = new List<GetUserRoleDto>
            {
                userRolesDto
            };

            return userRolesDtoList;

        }

        public async Task<List<GetUserRoleDto>> GetUserRolesByRoleNameQuery(string email)
        {

            Console.WriteLine($"Buscando roles del usuario con email: {email}");

            // Crear el filtro para buscar por UserId
            var filter = Builders<GetUserRoleDto>.Filter.Eq("RoleName", email);

            // Excluir el campo _id de la consulta
            var projection = Builders<GetUserRoleDto>.Projection.Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userRolesDto = await _collection
                .Find(filter)
                .Project<GetUserRoleDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(userRolesDto != null ? $"Roles encontrados: {userRolesDto}" : "No se encontraron roles para el usuario.");

            var userRolesDtoList = new List<GetUserRoleDto>
                                    {
                    userRolesDto
                };

            return userRolesDtoList;

        }

        public async Task<List<GetUserRoleDto>> GetUserRolesByUserEmailQuery(string email)
        {
            Console.WriteLine($"Buscando roles del usuario con email: {email}");

            // Crear el filtro para buscar por UserId
            var filter = Builders<GetUserRoleDto>.Filter.Eq("UserEmail", email);

            // Excluir el campo _id de la consulta
            var projection = Builders<GetUserRoleDto>.Projection.Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userRolesDto = await _collection
            .Find(filter)
                .Project<GetUserRoleDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(userRolesDto != null ? $"Roles encontrados: {userRolesDto}" : "No se encontraron roles para el usuario.");


            var userRolesDtoList = new List<GetUserRoleDto>
            {
                userRolesDto
            };

            return userRolesDtoList;
        }

        public async Task<GetUserRoleDto> GetRoleByIdAndByUserIdQuery(string roleId, string userId)
        {
            Console.WriteLine($"Buscando roles del usuario con ID: {userId}");
            Console.WriteLine($"Buscando roles del usuario con rol: {roleId}");
            // Crear el filtro para buscar por UserId
            var filter = Builders<GetUserRoleDto>.Filter.And(
                Builders<GetUserRoleDto>.Filter.Eq("UserEmail", userId),
                Builders<GetUserRoleDto>.Filter.Eq("RoleName", roleId)
            );
            // Excluir el campo _id de la consulta
            var projection = Builders<GetUserRoleDto>.Projection.Exclude("_id");
            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userRolesDto = await _collection
                .Find(filter)
                .Project<GetUserRoleDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            Console.WriteLine(userRolesDto != null ? $"Roles encontrados: {userRolesDto.UserRoleId}" : "No se encontraron roles para el usuario.");

            return userRolesDto;
        }

        public async Task<List<GetUserRoleDto>> GetUsersRoleAsync()
        {
            Console.WriteLine("Obteniendo roles de todos los usuarios...");

            // Excluir el campo _id de la consulta
            var projection = Builders<GetUserRoleDto>.Projection.Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userRolesDto = await _collection
                .Find(Builders<GetUserRoleDto>.Filter.Empty) // Obtener todos los registros
                .Project<GetUserRoleDto>(projection) // Convertir el resultado al DTO
                .ToListAsync()
                .ConfigureAwait(false);

            if (userRolesDto == null || userRolesDto.Count == 0)
            {
                Console.WriteLine("No se encontraron roles de usuarios.");
                return new List<GetUserRoleDto>(); // Retorna una lista vacía en lugar de `null` para evitar errores
            }


            return userRolesDto;
        }

    }
}
