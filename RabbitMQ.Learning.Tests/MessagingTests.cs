using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class MessagingTests
    {
        [Theory]
        [InlineData("rabbitmq.test.queue", "Hello, world!")]
        public void WhenAMessageIsPublishedOnAQueueThenAConsumerShouldReceiveIt(string queueName, string originalMessage)
        {
            new TestBuilder<MessagingContext>()
                .Given(() =>
                {
                    var factory = new ConnectionFactory { HostName = "localhost" };
                    var connection = factory.CreateConnection();

                    return new MessagingContext(() =>
                    {
                        var sender = connection.CreateModel();

                        sender.QueueDeclare(queueName, false, false, true, null);

                        return sender;
                    }, () =>
                    {
                        var receiver = connection.CreateModel();

                        receiver.QueueDeclare(queueName, false, false, true, null);

                        return receiver;
                    });
                })
                .When(context =>
                {
                    // sender
                    var body = Encoding.UTF8.GetBytes(originalMessage);

                    context.Sender.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                })
                .Then(context =>
                {
                    // receiver
                    var consumer = new EventingBasicConsumer(context.Receiver);

                    consumer.Received += (s, ea) =>
                    {
                        var body = ea.Body;
                        var receivedMessage = Encoding.UTF8.GetString(body);

                        Assert.Equal(originalMessage, receivedMessage);
                    };

                    context.Receiver.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                });
        }

        [Theory]
        [InlineData("rabbitmq.test.exchange", "rabbitmq.test.queue", "Hello, world!")]
        public void WhenAMessageIsPublishedOnAnExchangeThenAQueueBindedToItShouldReceiveThat(string exchange, string queueName, string originalMessage)
        {
            new TestBuilder<MessagingContext>()
                .Given(() =>
                {
                    var factory = new ConnectionFactory { HostName = "localhost" };
                    var connection = factory.CreateConnection();

                    return new MessagingContext(() =>
                    {
                        var sender = connection.CreateModel();

                        sender.ExchangeDeclare(exchange: exchange, type: "fanout");

                        return sender;
                    }, () =>
                    {
                        var receiver = connection.CreateModel();

                        receiver.ExchangeDeclare(exchange: exchange, type: "fanout");
                        receiver.QueueDeclare(queueName, false, false, true, null);
                        receiver.QueueBind(queue: queueName, exchange: exchange, routingKey: "");

                        return receiver;
                    });
                })
                .When(context =>
                {
                    // sender
                    var body = Encoding.UTF8.GetBytes(originalMessage);

                    context.Sender.BasicPublish(exchange: exchange, routingKey: "", basicProperties: null, body: body);
                })
                .Then(context =>
                {
                    // receiver
                    var consumer = new EventingBasicConsumer(context.Receiver);

                    consumer.Received += (s, ea) =>
                    {
                        var body = ea.Body;
                        var receivedMessage = Encoding.UTF8.GetString(body);

                        Assert.Equal(originalMessage, receivedMessage);
                    };

                    context.Receiver.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                });
        }
    }
}
