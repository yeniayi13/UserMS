using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Permission;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class PermissionConfigurationMongo
    {
        public static void Configure(IMongoCollection<Permissions> collection)
        {
            // Crear un índice único en PermissionName para evitar nombres duplicados
            var indexKeysDefinition = Builders<Permissions>.IndexKeys.Ascending(p => p.PermissionName.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Permissions>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en PermissionId para mejorar la búsqueda por ID
            indexKeysDefinition = Builders<Permissions>.IndexKeys.Ascending(p => p.PermissionId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en CreatedAt para ordenar por fecha de creación
            indexKeysDefinition = Builders<Permissions>.IndexKeys.Ascending(p => p.CreatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en UpdatedAt para mejorar rendimiento en consultas de modificaciones recientes
            indexKeysDefinition = Builders<Permissions>.IndexKeys.Ascending(p => p.UpdatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
