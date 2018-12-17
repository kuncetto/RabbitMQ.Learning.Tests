using RabbitMQ.Client;
using RabbitMQ.Learning.Tests.Contexts.Subscription;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class SubscriptionTests
    {
        [Theory]
        [InlineData("rabbitmq.test.queue", "Hello World!")]
        public void WhenPublisherSendsAMessageDirectlyToAQueueThenSubscriberShouldReceiveThat(string queue, string message)
        {
            // Publish directly on a queue (exchange = "", routingKey = queue)
            WhenPublisherSendsAMessageThenSubscriberShouldConsumeThat("", queue, queue, message);
        }

        [Theory]
        [InlineData("rabbitmq.test.exchange", "rabbitmq.test.queue", "Publish/Subscribe")]
        public void WhenPublisherSendsAMessageToAnExchangeThenABoundQueueShouldReceiveThat(string exchange, string queue, string message)
        {
            // Publish on exchange bound by a queue
            WhenPublisherSendsAMessageThenSubscriberShouldConsumeThat(exchange, "", queue, message);
        }

        private void WhenPublisherSendsAMessageThenSubscriberShouldConsumeThat(string exchange, string routingKey, string queue, string message)
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            {
                new TestBuilder<SubscriptionContext>()
                    .Given(() =>
                    {
                        return new SubscriptionContext(
                            publishers: new List<Publisher> { new Publisher(connection.CreateModel(), exchange, routingKey) },
                            subscribers: new List<Subscriber> { new  Subscriber(connection.CreateModel(), queue, exchange, routingKey) });
                    })
                    .When(context =>
                    {
                        context.Publishers.First().Publish(Encoding.UTF8.GetBytes(message));
                    })
                    .Then(context =>
                    {
                        var task = context.Subscribers.First().ConsumeAsync();
                        Assert.Equal(message, Encoding.UTF8.GetString(task.Result));
                    });
            }
        }
    }
}
