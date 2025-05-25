using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Bidder;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class BidderConfigurationMongo
    {
        public static void Configure(IMongoCollection<Bidders> collection)
        {
            // Crear un índice único en BidderDni para evitar duplicados
            var indexKeysDefinition = Builders<Bidders>.IndexKeys.Ascending(b => b.BidderDni.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Bidders>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en BidderId para mejorar la búsqueda por ID
           

            // Crear un índice en BidderUserId para facilitar búsquedas por usuario
            indexKeysDefinition = Builders<Bidders>.IndexKeys.Ascending(b => b.UserId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en IsDeleted para mejorar consultas por estado de eliminación
            indexKeysDefinition = Builders<Bidders>.IndexKeys.Ascending(b => (bool)b.BidderDelete.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en CreatedAt para ordenar por fecha de creación
            indexKeysDefinition = Builders<Bidders>.IndexKeys.Ascending(b => b.CreatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en UpdatedAt para mejorar rendimiento en consultas de actualizaciones
            indexKeysDefinition = Builders<Bidders>.IndexKeys.Ascending(b => b.UpdatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
