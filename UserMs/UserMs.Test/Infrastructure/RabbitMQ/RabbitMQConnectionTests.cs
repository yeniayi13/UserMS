using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Core.RabbitMQ;
using Xunit;

namespace UserMs.Test.Infrastructure.RabbitMQ
{
    public class RabbitMQConnectionTests
    {
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IChannel> _mockChannel;
        private readonly IConnectionRabbbitMQ _rabbitMQConnection;
        private readonly Mock<IConnectionFactory> _mockFactory;

        public RabbitMQConnectionTests()
        {
            _mockFactory = new Mock<IConnectionFactory>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IChannel>();

            // Simular la conexión y el canal en la fábrica
            _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c =>
                    c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockChannel.Object);

            // Pasar la fábrica simulada a RabbitMQConnection
            _rabbitMQConnection = new RabbitMQConnection(_mockFactory.Object);
        }


        // ✅ Verifica que `InitializeAsync` configura la conexión correctamente
        [Fact]
        public async Task InitializeAsync_ShouldEstablishConnectionAndChannel()
        {
            // Arrange

            _mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c =>
                    c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockChannel.Object);

            var rabbitMQConnection = new RabbitMQConnection(_mockFactory.Object);

            // Act
            await rabbitMQConnection.InitializeAsync();

            // Assert
            Assert.NotNull(rabbitMQConnection.GetChannel());
        }

        // ❌ Simulación de fallo en la conexión con RabbitMQ
        [Fact]
        public async Task InitializeAsync_Should_Throw_Exception_When_Channel_Creation_Fails()
        {
            _mockConnection.Setup(c =>
                    c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IChannel)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _rabbitMQConnection.InitializeAsync();
            });

            Assert.Equal("No se pudo crear el canal de comunicación con RabbitMQ.", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_Should_Throw_Exception_When_QueueDeclaration_Fails()
        {
            _mockConnection.Setup(c =>
                    c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockChannel.Object);

            _mockChannel.Setup(c => c.QueueDeclareAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new InvalidOperationException("Error al declarar la cola"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _rabbitMQConnection.InitializeAsync();
            });

            Assert.Equal("Error al declarar la cola", exception.Message);
        }

        [Fact]
        public async Task GetChannel_Should_Return_Channel_When_Initialized()
        {
            _mockConnection.Setup(c =>
                    c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockChannel.Object);

            await _rabbitMQConnection.InitializeAsync();
            var channel = _rabbitMQConnection.GetChannel();

            Assert.NotNull(channel);
        }

        [Fact]
        public void GetChannel_Should_Throw_Exception_When_Not_Initialized()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => _rabbitMQConnection.GetChannel());
            Assert.Equal("RabbitMQ aún no está inicializado correctamente.", exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_Should_Call_Methods_Correctly()
        {
            _mockConnection.Setup(c =>
                    c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockChannel.Object);

            _mockChannel.Setup(c => c.QueueDeclareAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new QueueDeclareOk(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>()));

            await _rabbitMQConnection.InitializeAsync();

            _mockConnection.Verify(
                c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockChannel.Verify(c => c.QueueDeclareAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()
            ), Times.Exactly(8));
        }

    }
}
