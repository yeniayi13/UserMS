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
        try
        {
            Console.WriteLine("Intentando conectar a RabbitMQ...");

            // Verifica que la instancia del ConnectionFactory no sea nula
            if (_connectionFactory == null)
            {
                throw new InvalidOperationException("La fábrica de conexiones de RabbitMQ no está disponible.");
            }

            // Intentar establecer la conexión
            _connection = await _connectionFactory.CreateConnectionAsync(CancellationToken.None);
            if (_connection == null)
            {
                throw new InvalidOperationException("No se pudo establecer la conexión con RabbitMQ.");
            }

            Console.WriteLine("Conexión con RabbitMQ establecida.");

            // Intentar crear el canal
            _channel = await _connection.CreateChannelAsync();
            if (_channel == null)
            {
                throw new InvalidOperationException("No se pudo crear el canal de comunicación con RabbitMQ.");
            }

            Console.WriteLine(" Canal de RabbitMQ creado correctamente.");

            // Lista de colas a declarar
            var queues = new List<string>
            {
                "userQueue", "supportQueue", "bidderQueue", "auctioneerQueue",
                "userRoleQueue", "roleQueue", "rolePermissionQueue", "activityHistoryQueue"
            };

            foreach (var queue in queues)
            {
                await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
                Console.WriteLine($" Cola '{queue}' declarada correctamente.");
            }

            Console.WriteLine(" Todas las colas de RabbitMQ han sido declaradas y están listas para su uso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la inicialización de RabbitMQ: {ex.Message}");
            throw; // Relanza la excepción para manejarla en niveles superiores si es necesario
        }
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