using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    static class IConnectionExtensions
    {
        public static IModel CreatePublisher(this IConnection connection, string exchange, string routingKey, string queue)
        {
            return connection.CreateModel().ForPublisher(exchange, routingKey, queue);
        }

        public static IModel CreateSubscriber(this IConnection connection, string exchange, string routingKey, string queue)
        {
            return connection.CreateModel().ForSubscriber(exchange, routingKey, queue);
        }
    }
}
