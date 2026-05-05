using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Model;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;

const string exchangeName = "pedido.exchange";
const string queueName = "pedido.criados";
const string routingKey = "pedido.criado";

static string GetEnv(string name, string fallback) =>
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
        ? fallback
        : Environment.GetEnvironmentVariable(name)!;

static int GetEnvInt(string name, int fallback) =>
    int.TryParse(Environment.GetEnvironmentVariable(name), out var v) ? v : fallback;

var factory = new ConnectionFactory
{
    HostName = GetEnv("RABBITMQ_HOST", "localhost"),
    Port = GetEnvInt("RABBITMQ_PORT", 5672),
    UserName = GetEnv("RABBITMQ_USERNAME", "guest"),
    Password = GetEnv("RABBITMQ_PASSWORD", "guest"),
    VirtualHost = GetEnv("RABBITMQ_VHOST", "/"),
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: ExchangeType.Direct,
    durable: true,
    autoDelete: false,
    arguments: null);

await channel.QueueDeclareAsync(
    queue: queueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

await channel.QueueBindAsync(
    queue: queueName,
    exchange: exchangeName,
    routingKey: routingKey,
    arguments: null);

await channel.BasicQosAsync(
    prefetchSize: 0,
    prefetchCount: 1,
    global: false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var json = System.Text.Encoding.UTF8.GetString(body);
        var pedido = JsonSerializer.Deserialize<Pedido>(json);

        Console.WriteLine("====================================");
        Console.WriteLine($"[Consumer] Pedido recebido: {pedido?.Id}");
        Console.WriteLine($"Cliente: {pedido?.ClienteEmail}");
        Console.WriteLine($"Valor: {pedido?.ValorTotal:C}");
        Console.WriteLine($"Criado: {pedido?.DataCriacao:O}");
        Console.WriteLine($"====================================");

        await Task.Delay(2000); // Simula o processamento do pedido

        // Confirma o processmento da mensagem
        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"[Consumer] Erro ao deserializar o pedido: {ex.Message}");
        await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        throw;
    }
    catch (Exception e)
    {
        Console.WriteLine($"[Consumer] Erro ao Processar o pedido: {e.Message}");
        await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        throw;
    }
};


await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Consumer iniciado. Pressione ENTER para sair");
Console.ReadLine();