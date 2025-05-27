using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.RabbitMQ;
using UserMs.Infrastructure.RabbitMQ;
using Xunit;

namespace UserMs.Test.Infrastructure.RabbitMQ
{
    public class RabbitMQProducerTests
    {
        private readonly Mock<IConnectionRabbbitMQ> _mockRabbitMQConnection;
        private readonly Mock<IChannel> _mockChannel;
        private readonly RabbitMQProducer<GetUsersDto> _producer;

        public RabbitMQProducerTests()
        {
            _mockRabbitMQConnection = new Mock<IConnectionRabbbitMQ>();
            _mockChannel = new Mock<IChannel>();

            // 🔹 Simular que GetChannel devuelve un canal mockeado
            _mockRabbitMQConnection.Setup(c => c.GetChannel()).Returns(_mockChannel.Object);

            // 🔹 Instanciar el productor con el mock de conexión
            _producer = new RabbitMQProducer<GetUsersDto>(_mockRabbitMQConnection.Object);
        }

        [Fact]
        public async Task PublishMessageAsync_ShouldDeclareQueue_AndPublishMessage()
        {
            // 🔹 Arrange
            var queueName = "testQueue";
            var eventType = "PRODUCT_CREATED";
            var testProduct = new GetUsersDto { UserId = Guid.NewGuid(), UserName = "Test Product" };

            // 🔹 Simular la declaración de la cola
            _mockChannel.Setup(c => c.QueueDeclareAsync(
                    queueName, true, false, false, It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk(queueName, 0, 0)); // 🔥 Devuelve un objeto válido

            // 🔹 Simular publicación del mensaje
            _mockChannel.Setup(c => c.BasicPublishAsync<BasicProperties>(
                "", queueName, false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()
            )).Returns(ValueTask.CompletedTask);
            // 🔹 Act
            await _producer.PublishMessageAsync(testProduct, queueName, eventType);

            // 🔹 Assert
            _mockChannel.Verify(c => c.QueueDeclareAsync(
                queueName, true, false, false, It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);

            _mockChannel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                "", queueName, false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]

        public async Task PublishMessageAsync_ShouldThrowException_WhenMessageIsNull()
        {
            var queueName = "testQueue";
            var eventType = "PRODUCT_CREATED";
            GetUsersDto testProduct = null; // 🔥 Simular mensaje nulo

            // 🔹 Verificar que se lanza la excepción al intentar publicar un mensaje vacío
            await Assert.ThrowsAsync<ArgumentNullException>(() => _producer.PublishMessageAsync(testProduct, queueName, eventType));
        }

        [Fact]
        public async Task PublishMessageAsync_ShouldHandleChannelFailure()
        {
            var queueName = "testQueue";
            var eventType = "PRODUCT_CREATED";
            var testProduct = new GetUsersDto { UserId = Guid.NewGuid(), UserName = "Test Product" };

            _mockChannel.Setup(c => c.BasicPublishAsync<BasicProperties>(
                "", queueName, false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Error en RabbitMQ"));

            await Assert.ThrowsAsync<Exception>(() => _producer.PublishMessageAsync(testProduct, queueName, eventType));

            _mockChannel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                "", queueName, false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublishMessageAsync_ShouldSupportDifferentEventTypes()
        {
            var queueName = "testQueue";
            var eventType = "PRODUCT_UPDATED"; // 🔥 Diferente tipo de evento
            var testProduct = new GetUsersDto { UserId = Guid.NewGuid(), UserName = "Updated Product" };

            _mockChannel.Setup(c => c.QueueDeclareAsync(
                queueName, true, false, false, It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new QueueDeclareOk(queueName, 0, 0));

            _mockChannel.Setup(c => c.BasicPublishAsync<BasicProperties>(
                "", queueName, false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()
            )).Returns(ValueTask.CompletedTask);

            await _producer.PublishMessageAsync(testProduct, queueName, eventType);

            _mockChannel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                "", queueName, false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
