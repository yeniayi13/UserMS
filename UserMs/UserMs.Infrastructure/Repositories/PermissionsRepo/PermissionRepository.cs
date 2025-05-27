using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.Database;
using UserMs.Core.Repositories.PermissionRepo;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Infrastructure.Repositories.PermissionsRepo
{
    public class PermissionRepository : IPermissionRepositoryMongo
    {

     
        private readonly IMongoCollection<Permissions> _collection;
        private readonly IMapper _mapper;


        public PermissionRepository( IUserDbContextMongo context, IMapper mapper)
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
            _collection = context.Database.GetCollection<Permissions>("Permissions");
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }

        public async Task<List<Permissions>> GetPermissionAllQueryAsync()
        {
            var projection = Builders<Permissions>.Projection.Exclude("_id")
                                                            .Exclude("CreatedAt")
                                                            .Exclude("CreatedBy");// Excluir _id

            Console.WriteLine("Consulta en proceso...");

            var permissionsDto = await _collection
            .Find(Builders<Permissions>.Filter.Empty)
                .Project<GetPermissionDto>(projection) // Convierte los datos al DTO
                .ToListAsync()
                .ConfigureAwait(false);

            if (permissionsDto == null || permissionsDto.Count == 0)
            {
                Console.WriteLine("No se encontraron permisos.");
                return new List<Permissions>(); // Retorna una lista vacía en lugar de `null` para evitar errores
            }

            var permissionsEntity = _mapper.Map<List<Permissions>>(permissionsDto);
            return permissionsEntity;
        }

        public async Task<Permissions?> GetPermissionByIdAsync(Guid permissionId)
        {
            Console.WriteLine($"Buscando roles con permisos con ID: {permissionId}");

            // Crear el filtro para buscar roles por UserId
            var filter = Builders<Permissions>.Filter.Eq("PermissionId", permissionId);

            // Excluir el campo _id de la consulta
            var projection = Builders<Permissions>.Projection
                .Exclude("_id")
                .Exclude("CreatedAt")
                .Exclude("CreatedBy");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var permissionsDto = await _collection
                .Find(filter)
                .Project<GetPermissionDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(permissionsDto != null ? $"Roles encontrados: {permissionsDto}" : "No se encontraron roles para el usuario.");

            var rolePermissionsEntity = _mapper.Map<Permissions>(permissionsDto);

            return rolePermissionsEntity;
        }
    }
}
