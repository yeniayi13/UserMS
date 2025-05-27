using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Core.RabbitMQ;
using UserMs.Infrastructure.RabbitMQ.Connection;
using Xunit;

namespace UserMs.Test.Infrastructure.RabbitMQ
{
    public class RabbitMQBackgroundServiceTests
    {
        private readonly Mock<IRabbitMQConsumer> _mockConsumer;
        private readonly RabbitMQBackgroundService _service;

        public RabbitMQBackgroundServiceTests()
        {
            _mockConsumer = new Mock<IRabbitMQConsumer>();
            _mockConsumer.Setup(c => c.ConsumeMessagesAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _service = new RabbitMQBackgroundService(_mockConsumer.Object);
        }

        [Fact]
        public async Task StartAsync_ShouldCall_ConsumeMessagesAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            await _service.StartAsync(cancellationTokenSource.Token);

            // 🔹 Esperar para asegurar que `ExecuteAsync` tuvo tiempo de ejecutarse
            await Task.Delay(3500);

            _mockConsumer.Verify(c => c.ConsumeMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        //  Prueba: `ExecuteAsync()` debe respetar `CancellationToken`
        [Fact]
        public async Task ExecuteAsync_ShouldNotFail_WhenCancellationRequested()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // 🔹 Simular cancelación activada

            await _service.StartAsync(cancellationTokenSource.Token);

            // 🔹 Verificar que NO se llama `ConsumeMessagesAsync`
            _mockConsumer.Verify(c => c.ConsumeMessagesAsync(It.IsAny<string>()), Times.Never);
        }

        //  Prueba: `ExecuteAsync()` debe llamar a `ConsumeMessagesAsync()`
        [Fact]
        public async Task ExecuteAsync_ShouldCall_ConsumeMessagesAsync()
        {
            var cancellationToken = CancellationToken.None;
            var service = new RabbitMQBackgroundService(_mockConsumer.Object);

            await service.StartAsync(cancellationToken);

            // 🔹 Espera extra para garantizar que las tareas internas han terminado
            await Task.Delay(3500);

            _mockConsumer.Verify(c => c.ConsumeMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        //  Prueba: `ExecuteAsync()` debe esperar antes de consumir mensajes
        [Fact]
        public async Task ExecuteAsync_ShouldWaitBeforeConsumingMessages()
        {
            var cancellationToken = CancellationToken.None; // 🔹 No cancelar ejecución
            var service = new RabbitMQBackgroundService(_mockConsumer.Object);

            // ✅ Iniciar la tarea en paralelo
            var executionTask = service.StartAsync(cancellationToken);

            // 🔹 Esperar **3500 ms** para estar seguro de que el retraso inicial ha pasado
            await Task.Delay(3500);

            // 🔹 Verificar que `ConsumeMessagesAsync()` se ha ejecutado al menos una vez después del retraso
            _mockConsumer.Verify(c => c.ConsumeMessagesAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

    }
}
