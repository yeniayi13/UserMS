using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Core.Database;

namespace UserMs.Infrastructure.Database.Context.Mongo
{
    [ExcludeFromCodeCoverage]
    public class UserDbContextMongo : IUserDbContextMongo
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;
        private IClientSessionHandle _session;
        private string connectionString;

        public UserDbContextMongo(string connectionString, string databaseName)
        {
            try
            {
                _client = new MongoClient(connectionString);
                _database = _client.GetDatabase(databaseName);
                ConfigureCollections();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error initializing database context: {e.Message}");
            }
        }

        public UserDbContextMongo(string connectionString)
        {
            this.connectionString = connectionString;
            _client = new MongoClient(connectionString);
        }

        public IMongoDatabase Database => _database;

        public IClientSessionHandle BeginTransaction()
        {
            _session = _client.StartSession();
            _session.StartTransaction();
            return _session;
        }

        public void CommitTransaction()
        {
            _session.CommitTransaction();
            Dispose();
        }

        public void AbortTransaction()
        {
            _session.AbortTransaction();
            Dispose();
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        private void ConfigureCollections()
        {
            try
            {
                var collectionNames = new[]
                {
                    "Users", "Roles", "Permissions","RolePermissions", "UserRoles","Supports","Bidders","Auctioneers","ActivityHistories",
                };

                var existingCollections = new HashSet<string>(_database.ListCollectionNames().ToEnumerable());
                Console.WriteLine("Existing collections: " + string.Join(", ", existingCollections));

                foreach (var collectionName in collectionNames)
                {
                    if (!existingCollections.Contains(collectionName))
                    {
                        _database.CreateCollection(collectionName);
                        Console.WriteLine($"Collection '{collectionName}' created.");
                    }
                    else
                    {
                        Console.WriteLine($"Collection '{collectionName}' already exists.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error configuring collections: {e.Message}");
            }
        }
    }

}
