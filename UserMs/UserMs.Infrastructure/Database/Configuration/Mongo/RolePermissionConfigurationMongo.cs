using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role_Permission;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class RolePermissionConfigurationMongo
    {
        public static void Configure(IMongoCollection<RolePermissions> collection)
        {
            // Crear un índice compuesto en RoleId y PermissionId para optimizar búsquedas de permisos asignados a roles
            var indexKeysDefinition = Builders<RolePermissions>.IndexKeys
                .Ascending(rp => rp.RoleId.Value)
                .Ascending(rp => rp.PermissionId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en RolePermissionId para identificación rápida
            indexKeysDefinition = Builders<RolePermissions>.IndexKeys.Ascending(rp => rp.RolePermissionId.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<RolePermissions>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en CreatedAt para mejorar consultas ordenadas por fecha de creación
            indexKeysDefinition = Builders<RolePermissions>.IndexKeys.Ascending(rp => rp.CreatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en UpdatedAt para optimizar búsquedas de modificaciones recientes
            indexKeysDefinition = Builders<RolePermissions>.IndexKeys.Ascending(rp => rp.UpdatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
