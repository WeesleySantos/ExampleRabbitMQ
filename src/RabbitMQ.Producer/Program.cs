using RabbitMQ.Client;
using RabbitMQ.Model;
using System.Text.Json;

const string exchangeName = "pedido.exchange";
const string queueName = "pedido.criados";
const string routingKey = "pedido.criado";

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
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

if (!int.TryParse(Console.ReadLine(), out var quantidadePedidos))
{
    quantidadePedidos = 3;
}

for (int i = 1; i < quantidadePedidos; i++)
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

    Console.WriteLine("Pressione ENTER para enviar o pr�ximo pedido...");
    Console.ReadLine();
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
