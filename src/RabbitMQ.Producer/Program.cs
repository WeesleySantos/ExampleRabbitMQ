using RabbitMQ.Client;
using RabbitMQ.Model;
using System.Text.Json;

const string exchangeName = "pedido.exchange";
const string queueName = "pedido.criados";
const string routingKey = "pedido.criado";

static string GetEnv(string name, string fallback) =>
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
        ? fallback
        : Environment.GetEnvironmentVariable(name)!;

static int GetEnvInt(string name, int fallback) =>
    int.TryParse(Environment.GetEnvironmentVariable(name), out var v) ? v : fallback;

static bool GetEnvBool(string name, bool fallback) =>
    bool.TryParse(Environment.GetEnvironmentVariable(name), out var v) ? v : fallback;

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

Console.WriteLine("Quantos pedidos você quer enviar?");

var quantidadePedidos = GetEnvInt("QUANTIDADE_PEDIDOS", 0);
if (quantidadePedidos <= 0 && !int.TryParse(Console.ReadLine(), out quantidadePedidos))
{
    quantidadePedidos = 3;
}

var autoSend = GetEnvBool("AUTO_SEND", false);

for (int i = 1; i <= quantidadePedidos; i++)
{
    var pedido = CriarPedidoFake(i);
    var body = JsonSerializer.SerializeToUtf8Bytes(pedido);
    var properties = new BasicProperties
    {
        Persistent = true,
        ContentType = "application/json",
        ContentEncoding = "utf-8"
    };

    await channel.BasicPublishAsync(
        exchange: exchangeName,
        routingKey: routingKey,
        mandatory: false,
        basicProperties: properties,
        body: body);

    Console.WriteLine($"[x] Pedido {pedido.Id} enviado" );

    if (!autoSend && i < quantidadePedidos)
    {
        Console.WriteLine("Pressione ENTER para enviar o próximo pedido...");
        Console.ReadLine();
    }
}

static Pedido CriarPedidoFake(int index)
{
    return new Pedido
    {
        Id = Guid.NewGuid(),
        ClienteEmail = $"{index}@gmail.com",
        ValorTotal = Random.Shared.Next(100, 5000),
        DataCriacao = DateTime.UtcNow,
        Itens = new List<Item>
        {
            new Item
            {
                NomeProduto = $"Produto {index}",
                Quantidade = Random.Shared.Next(1, 5),
                PrecoUnitario = Random.Shared.Next(20, 1000)
            },
        }
    };
}
