using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class RoleConfigurationMongo
    {
        public static void Configure(IMongoCollection<Roles> collection)
        {
            // Crear un índice único en RoleName para evitar duplicados
            var indexKeysDefinition = Builders<Roles>.IndexKeys.Ascending(r => r.RoleName.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Roles>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en RoleId para mejorar la búsqueda por ID
            indexKeysDefinition = Builders<Roles>.IndexKeys.Ascending(r => r.RoleId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en IsDeleted para mejorar consultas por estado de eliminación
            indexKeysDefinition = Builders<Roles>.IndexKeys.Ascending(r => (bool)r.IsDeleted.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en CreatedAt para ordenar por fecha de creación
            indexKeysDefinition = Builders<Roles>.IndexKeys.Ascending(r => r.CreatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en UpdatedAt para mejorar rendimiento en consultas de actualizaciones
            indexKeysDefinition = Builders<Roles>.IndexKeys.Ascending(r => r.UpdatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
