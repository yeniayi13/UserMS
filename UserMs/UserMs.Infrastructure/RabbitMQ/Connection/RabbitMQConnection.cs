using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

public class RabbitMQConnection
{
    private IConnection _connection;
    private IChannel _channel;

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = await factory.CreateConnectionAsync();

        if (_connection == null)
        {
            throw new InvalidOperationException("No se pudo establecer la conexión con RabbitMQ.");
        }

        _channel = await _connection.CreateChannelAsync();

        if (_channel == null)
        {
            throw new InvalidOperationException("No se pudo crear el canal de comunicación con RabbitMQ.");
        }

        await _channel.QueueDeclareAsync(queue: "userQueue", durable: true, exclusive: false, autoDelete: false);
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