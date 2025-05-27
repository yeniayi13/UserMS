using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class UserConfigurationMongo
    {
        public static void Configure(IMongoCollection<Users> collection)
        {
            // Crear un índice único en UserEmail para evitar duplicados
            var indexKeysDefinition = Builders<Users>.IndexKeys.Ascending(u => u.UserEmail.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Users>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en UserId para mejorar la búsqueda por ID
            indexKeysDefinition = Builders<Users>.IndexKeys.Ascending(u => u.UserId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // **Índice en UserAvailable considerando que es un ENUM**
            indexKeysDefinition = Builders<Users>.IndexKeys.Ascending(u => (int)u.UserAvailable);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // **Índice adicional en UserEmail para mejorar consultas frecuentes por correo**
            indexKeysDefinition = Builders<Users>.IndexKeys.Ascending(u => u.UserEmail.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
