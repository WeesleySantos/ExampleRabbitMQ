namespace RabbitMQ.Model
{
    public class Item
    {
        public required string NomeProduto { get; set; }
        public required int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
