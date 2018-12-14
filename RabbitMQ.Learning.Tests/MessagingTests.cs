using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class MessagingTests
    {
        [Theory]
        [InlineData("rabbitmq.test.queue", "Hello World!")]
        public void WhenAMessageIsPublishedOnAQueueThenAConsumerShouldReceiveIt(string queue, string message)
        {
            // Publish directly on a queue (exchange = "", routingKey = queue)
            WhenPublisherSendAMessageThenConsumerShouldReceiveIt("", queue, queue, message);
        }

        [Theory]
        [InlineData("rabbitmq.test.exchange", "rabbitmq.test.queue", "Publish/Subscribe")]
        public void WhenAMessageIsPublishedOnAnExchangeThenABoundQueueShouldReceiveIt(string exchange, string queue, string message)
        {
            // Publish on exchange bound by a queue
            WhenPublisherSendAMessageThenConsumerShouldReceiveIt(exchange, "", queue, message);
        }

        private void WhenPublisherSendAMessageThenConsumerShouldReceiveIt(string exchange, string routingKey, string queue, string message)
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            {
                new TestBuilder<MessagingContext>()
                    .Given(() =>
                    {
                        return new MessagingContext(
                            () => connection.CreateModel().ForPublisher(exchange, routingKey, queue),
                            () => connection.CreateModel().ForSubscriber(exchange, routingKey, queue));
                    })
                    .When(context =>
                    {
                        context.Publishers.First().Publish(exchange: exchange, routingKey: routingKey, message: message);
                    })
                    .Then(context =>
                    {
                        context.Subscribers.First().ConsumeWithTimeout(
                            queue: queue,
                            timeout: TimeSpan.FromSeconds(5),
                            onReceived: receivedMessage => Assert.Equal(message, receivedMessage),
                            onError: exception => throw exception,
                            onTimeout: timeout => throw new TimeoutException($"Timeout expired after {timeout} seconds!"));
                    });
            }
        }

        [Theory]
        [InlineData("rabbitmq.test.exchange", "", "rabbitmq.test.queue", "Publish/Subscribe Async")]
        public void WhenPublisherSendAMessageThenConsumerShouldReceiveItAsync(string exchange, string routingKey, string queue, string message)
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            {
                new TestBuilder<MessagingContext>()
                    .Given(() =>
                    {
                        return new MessagingContext(
                            publishers: new List<IModel> { connection.CreatePublisher(exchange, routingKey, queue) },
                            subscribers: new List<IModel> { connection.CreateSubscriber(exchange, routingKey, queue) });
                    })
                    .When(context =>
                    {
                        context.Publishers.First().Publish(exchange: exchange, routingKey: routingKey, message: message);
                    })
                    .Then(context =>
                    {
                        var received = context.Subscribers.First().ConsumeAsync(queue: queue).Result;
                        Assert.Equal(message, received);
                    });
            }
        }
    }
}
