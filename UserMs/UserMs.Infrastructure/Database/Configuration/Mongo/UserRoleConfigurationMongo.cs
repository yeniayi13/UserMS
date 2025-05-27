using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.User_Roles;

namespace UserMs.Infrastructure.Database.Configuration.Mongo
{
    [ExcludeFromCodeCoverage]
    public class UserRoleConfigurationMongo
    {
        public static void Configure(IMongoCollection<UserRoles> collection)
        {
            // Crear un índice único en UserRoleId para evitar duplicados
            var indexKeysDefinition = Builders<UserRoles>.IndexKeys.Ascending(ur => ur.UserRoleId.Value);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<UserRoles>(indexKeysDefinition, indexOptions);
            collection.Indexes.CreateOne(indexModel);

            // Crear un índice en UserId para mejorar consultas de roles asignados a usuarios
            indexKeysDefinition = Builders<UserRoles>.IndexKeys.Ascending(ur => ur.UserId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);

            // Crear un índice en RoleId para agilizar búsquedas de usuarios con ciertos roles
            indexKeysDefinition = Builders<UserRoles>.IndexKeys.Ascending(ur => ur.RoleId.Value);
            collection.Indexes.CreateOne(indexKeysDefinition);
        }
    }
}
