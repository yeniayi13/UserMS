using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using UserMs.Core.RabbitMQ;

public class RabbitMQConnection : IConnectionRabbbitMQ
{
    private IConnection _connection;
    private IChannel _channel;
    private readonly IConnectionFactory _connectionFactory;

    public RabbitMQConnection(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        // 🔹 Usa la instancia inyectada en el constructor
        _connection = await _connectionFactory.CreateConnectionAsync(CancellationToken.None);

        if (_connection == null)
        {
            throw new InvalidOperationException("No se pudo establecer la conexión con RabbitMQ.");
        }

        _channel = await _connection.CreateChannelAsync();

        if (_channel == null)
        {
            throw new InvalidOperationException("No se pudo crear el canal de comunicación con RabbitMQ.");
        }

        await _channel.QueueDeclareAsync("userQueue", true, false, false);
    }

    public IChannel GetChannel()
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ aún no está inicializado correctamente.");
        }

        return _channel;
    }

}