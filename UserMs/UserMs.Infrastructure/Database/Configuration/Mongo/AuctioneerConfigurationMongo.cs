using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Auctioneer;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class AuctioneerConfigurationMongo
    {
        public static void Configure(IMongoCollection<Auctioneers> collection)
        {
            // Crear un índice único en AuctioneerDni para evitar duplicados
            var indexKeysDefinition = Builders<Auctioneers>.IndexKeys.Ascending(a => a.AuctioneerDni.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Auctioneers>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en AuctioneerId para mejorar la búsqueda por ID
          /*  indexKeysDefinition = Builders<Auctioneers>.IndexKeys.Ascending(a => a.AuctioneerId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);*/

            // Crear un índice en AuctioneerUserId para facilitar búsquedas por usuario
            indexKeysDefinition = Builders<Auctioneers>.IndexKeys.Ascending(a => a.UserId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en IsDeleted para mejorar consultas por estado de eliminación
            indexKeysDefinition = Builders<Auctioneers>.IndexKeys.Ascending(a => (bool)a.AuctioneerDelete.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en CreatedAt para ordenar por fecha de creación
            indexKeysDefinition = Builders<Auctioneers>.IndexKeys.Ascending(a => a.CreatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en UpdatedAt para mejorar rendimiento en consultas de actualizaciones
            indexKeysDefinition = Builders<Auctioneers>.IndexKeys.Ascending(a => a.UpdatedAt);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
