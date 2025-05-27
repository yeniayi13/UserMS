using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Core.Database;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Infrastructure.Repositories.RolesRepo
{
    public class RoleRepository: IRolesRepository
    {
       
        private readonly IMongoCollection<Roles> _collection;
        private readonly IMapper _mapper;

        public RoleRepository( IUserDbContextMongo context, IMapper mapper)
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
            _collection = context.Database.GetCollection<Roles>("Roles");
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));

        }


        public async Task<List<Roles>> GetRolesAllQueryAsync()
        {
            var projection = Builders<Roles>.Projection.Exclude("_id").Exclude("IsDeleted")
                .Exclude("CreatedAt")
                .Exclude("CreatedBy"); // Excluir _id

            Console.WriteLine("Consulta en proceso...");

            var rolesDto = await _collection
            .Find(Builders<Roles>.Filter.Empty)
                .Project<GetRoleDto>(projection) // Convierte los datos al DTO
                .ToListAsync()
                .ConfigureAwait(false);

            if (rolesDto == null || rolesDto.Count == 0)
            {
                Console.WriteLine("No se encontraron roles.");
                return new List<Roles>(); // Retorna una lista vacía en lugar de `null` para evitar errores
            }

            var rolesEntity = _mapper.Map<List<Roles>>(rolesDto);

            return rolesEntity;
        }

        public async Task<Roles?> GetRolesByIdQuery(Guid roleId)
        {
            Console.WriteLine($"Buscando rol con ID: {roleId}");

            // Crear el filtro para buscar por RoleId
            var filter = Builders<Roles>.Filter.Eq("RoleId", roleId);

            // Excluir el campo _id de la consulta
            var projection = Builders<Roles>.Projection
                .Exclude("_id")
                .Exclude("IsDeleted")
                .Exclude("CreatedAt")
                .Exclude("CreatedBy");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var roleDto = await _collection
                .Find(filter)
                .Project<GetRoleDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(roleDto != null ? $"Rol encontrado: {roleDto}" : "Rol no encontrado.");

            var rolesEntity = _mapper.Map<Roles>(roleDto);

            return rolesEntity;
        }

        public async Task<Roles?> GetRolesByNameQuery(string roleName)
        {
            Console.WriteLine($"Buscando rol con nombre: {roleName}");

            // Crear el filtro para buscar por RoleName
            var filter = Builders<Roles>.Filter.Eq("RoleName", roleName);

            // Excluir el campo _id de la consulta
            var projection = Builders<Roles>.Projection
                .Exclude("_id")
                .Exclude("IsDeleted")
                .Exclude("CreatedAt")
                .Exclude("CreatedBy");

            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var roleDto = await _collection
                .Find(filter)
                .Project<GetRoleDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            Console.WriteLine(roleDto != null ? $"Rol encontrado: {roleDto}" : "Rol no encontrado.");

            var rolesEntity = _mapper.Map<Roles>(roleDto);

            return rolesEntity;
        }


      

    }
    
}
