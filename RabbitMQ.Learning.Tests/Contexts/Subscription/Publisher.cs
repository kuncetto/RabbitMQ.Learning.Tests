using RabbitMQ.Client;

namespace RabbitMQ.Learning.Tests.Contexts.Subscription
{
    class Publisher
    {
        public IModel Model { get; private set; }
        public string Exchange { get; private set; }
        public string RoutingKey { get; private set; }

        public Publisher(IModel model, string exchange, string routingKey = default(string))
        {
            Model = model;
            Exchange = exchange;
            RoutingKey = routingKey ?? string.Empty;

            if (!string.IsNullOrEmpty(exchange))
            {
                model.ExchangeDeclare(exchange: exchange, type: "fanout");
            }
        }

        public void Publish(byte[] body)
        {
            Model.BasicPublish(exchange: Exchange, routingKey: RoutingKey, basicProperties: null, body: body);
        }
    }
}
