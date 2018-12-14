using RabbitMQ.Client;

namespace RabbitMQ.Learning.Tests.Extensions
{
    static class IConnectionExtensions
    {
        public static IModel CreatePublisher(this IConnection connection, string exchange)
        {
            return connection.CreateModel().ForPublisher(exchange);
        }

        public static IModel CreateSubscriber(this IConnection connection, string exchange, string routingKey, string queue)
        {
            return connection.CreateModel().ForSubscriber(exchange, routingKey, queue);
        }
    }
}
