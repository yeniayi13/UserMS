using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.ActivityHistory;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class ActivityHistoryConfigurationMongo
    {
        public static void Configure(IMongoCollection<ActivityHistory> collection)
        {
            // Índice en UserId para facilitar la búsqueda por usuario
            var indexKeysDefinition = Builders<ActivityHistory>.IndexKeys.Ascending(a => a.UserId.Value);
            collection.Indexes.CreateOne(new CreateIndexModel<ActivityHistory>(indexKeysDefinition));

            // Índice en Timestamp para mejorar la consulta por fecha
            indexKeysDefinition = Builders<ActivityHistory>.IndexKeys.Ascending(a => a.Timestamp);
            collection.Indexes.CreateOne(new CreateIndexModel<ActivityHistory>(indexKeysDefinition));

            // Índice en Action para consultas rápidas por tipo de acción
            indexKeysDefinition = Builders<ActivityHistory>.IndexKeys.Ascending(a => a.Action);
            collection.Indexes.CreateOne(new CreateIndexModel<ActivityHistory>(indexKeysDefinition));
        }
    }
}
