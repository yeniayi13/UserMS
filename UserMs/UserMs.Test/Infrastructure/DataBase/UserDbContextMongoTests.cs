using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Infrastructure.Database.Context.Mongo;
using Xunit;

namespace UserMs.Test.Infrastructure.DataBase
{
    public class UserDbContextMongoTests
    {
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IClientSessionHandle> _sessionMock;
        private UserDbContextMongo _context;

        public UserDbContextMongoTests()
        {
            _mongoClientMock = new Mock<IMongoClient>();
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _sessionMock = new Mock<IClientSessionHandle>();

            // Configurar el comportamiento simulado de la sesión
            _sessionMock.Setup(s => s.StartTransaction(default));
            _sessionMock.Setup(s => s.CommitTransaction(default));
            _sessionMock.Setup(s => s.AbortTransaction(default));

            // Configurar el cliente de MongoDB simulado
            _mongoClientMock.Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                .Returns(_mongoDatabaseMock.Object);

            // Inicializar el contexto y comenzar la sesión
            _context = new UserDbContextMongo("mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "TestDatabase");
            _context.BeginTransaction();
        }
        [Fact]
        public void Constructor_ShouldInitializeDatabaseContext()
        {
            // Act
            _context = new UserDbContextMongo("mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "TestDatabase");

            // Assert
            Assert.NotNull(_context);
            Assert.NotNull(_context.Database);
        }

        [Fact]
        public void Constructor_ShouldHandleException_Gracefully()
        {
            // Arrange
            var invalidConnectionString = "invalid_connection_string";

            // Act & Assert
            var exception = Record.Exception(() => new UserDbContextMongo(invalidConnectionString, "TestDatabase"));
            Assert.Null(exception); // No debe lanzar excepción
        }
        [Fact]
        public void Database_ShouldReturnMongoDatabase()
        {
            // Arrange
            _context = new UserDbContextMongo("mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "TestDatabase");

            // Act
            var database = _context.Database;

            // Assert
            Assert.NotNull(database);
        }

       
        [Fact]
        public void BeginTransaction_ShouldStartSession()
        {
            // Arrange
            _sessionMock.Setup(s => s.StartTransaction(default));

            // Act
            var session = _context.BeginTransaction();

            // Assert
            Assert.NotNull(session);
        }

      

        [Fact]
        public void ConfigureCollections_ShouldHandleException_Gracefully()
        {
            // Arrange
            _mongoDatabaseMock.Setup(db => db.ListCollectionNames(null,default)).Throws(new Exception("DB Error"));

            // Act
            var exception = Record.Exception(() =>
                _context = new UserDbContextMongo("mongodb+srv://yadefreitas19:08092001@cluster0.owy2d.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "TestDatabase"));

            // Assert
            Assert.Null(exception); // No debe lanzar excepción
        }
    }
}

