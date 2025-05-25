using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Support;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class SupportConfigurationMongo
    {
        public static void Configure(IMongoCollection<Supports> collection)
        {
            // Crear un índice único en SupportDni para evitar duplicados
            var indexKeysDefinition = Builders<Supports>.IndexKeys.Ascending(s => s.SupportDni.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Supports>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

           

            // Crear un índice en SupportUserId para facilitar búsquedas por usuario
            indexKeysDefinition = Builders<Supports>.IndexKeys.Ascending(s => s.UserId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en IsDeleted para mejorar consultas por estado de eliminación
            indexKeysDefinition = Builders<Supports>.IndexKeys.Ascending(s => (bool)s.SupportDelete.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en CreatedAt para ordenar por fecha de creación
            indexKeysDefinition = Builders<Supports>.IndexKeys.Ascending(s => s.CreatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en UpdatedAt para mejorar rendimiento en consultas de actualizaciones
            indexKeysDefinition = Builders<Supports>.IndexKeys.Ascending(s => s.UpdatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
