using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.Database;
using UserMs.Core.Repositories.RolePermissionRepo;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;

namespace UserMs.Infrastructure.Repositories.Roles_Permission
{
    public class RolePermissionRepositoryMongo : IRolePermissionRepositoryMongo
    {

     
        private readonly IMongoCollection<GetRolePermissionDto> _collection;
        private readonly IMapper _mapper;
        public RolePermissionRepositoryMongo( IUserDbContextMongo context, IMapper mapper)
        {
            
            if (context.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _collection = context.Database.GetCollection<GetRolePermissionDto>("RolePermissions");
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }

       
        public async Task<GetRolePermissionDto?> GetRolesPermissionByRoleQuery(string roleName)
        {
            Console.WriteLine($"Buscando roles y permisos con el rol de  ID: {roleName}");

            // Crear el filtro para buscar roles por UserId
            var filter = Builders<GetRolePermissionDto>.Filter.Eq("RoleName", roleName);

            // Excluir el campo _id de la consulta
            var projection = Builders<GetRolePermissionDto>.Projection
                .Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var rolePermissionsDto = await _collection
                .Find(filter)
                .Project<GetRolePermissionDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(rolePermissionsDto != null ? $"Roles encontrados: {rolePermissionsDto}" : "No se encontraron roles para el usuario.");


            return rolePermissionsDto;
        }

        public async Task<List<GetRolePermissionDto>> GetRolesPermissionAsync()
        {
            Console.WriteLine("Obteniendo roles de todos los usuarios...");

            // Excluir el campo _id de la consulta
            var projection = Builders<GetRolePermissionDto>.Projection.Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var rolePermissionsDto = await _collection
                .Find(Builders<GetRolePermissionDto>.Filter.Empty) // Obtener todos los registros
                .Project<GetRolePermissionDto>(projection) // Convertir el resultado al DTO
                .ToListAsync()
                .ConfigureAwait(false);

            if (rolePermissionsDto == null || rolePermissionsDto.Count == 0)
            {
                Console.WriteLine("No se encontraron roles de usuarios.");
                return new List<GetRolePermissionDto>(); // Retorna una lista vacía en lugar de `null` para evitar errores
            }


            return rolePermissionsDto;
        }

        public async Task<GetRolePermissionDto?> GetRolesPermissionByIdQuery(Guid rolePermissionId)
        {
            Console.WriteLine($"Buscando roles con permisos con ID: {rolePermissionId}");

            // Crear el filtro para buscar roles por UserId
            var filter = Builders<GetRolePermissionDto>.Filter.Eq("RolePermissionId", rolePermissionId);

            // Excluir el campo _id de la consulta
            var projection = Builders<GetRolePermissionDto>.Projection
                .Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var rolePermissionsDto = await _collection
                .Find(filter)
                .Project<GetRolePermissionDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(rolePermissionsDto != null ? $"Roles encontrados: {rolePermissionsDto}" : "No se encontraron roles para el usuario.");


            return rolePermissionsDto;
        }

        public async Task<GetRolePermissionDto?> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId)
        {
            Console.WriteLine($"Buscando rol con permiso. RoleId: {roleId}, PermissionId: {permissionId}");

            // Crear el filtro para buscar por RoleId y PermissionId
            var filter = Builders<GetRolePermissionDto>.Filter.And(
                Builders<GetRolePermissionDto>.Filter.Eq("RoleId", roleId),
                Builders<GetRolePermissionDto>.Filter.Eq("PermissionId", permissionId)
            );

            // Excluir el campo _id si no es necesario
            var projection = Builders<GetRolePermissionDto>.Projection.Exclude("_id");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var rolePermissionsDto = await _collection
                .Find(filter)
                .Project<GetRolePermissionDto>(projection) // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(rolePermissionsDto != null
                ? $"Rol encontrado: {rolePermissionsDto.RoleName}, Permiso: {rolePermissionsDto.PermissionName}"
                : "No se encontró la combinación de rol y permiso.");

            return rolePermissionsDto;
        }



    }
}
