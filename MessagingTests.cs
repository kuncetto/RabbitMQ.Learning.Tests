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
            new TestBuilder<IModel>()
                .Given(() =>
                {
                    var factory = new ConnectionFactory { HostName = "localhost" };
                    var connection = factory.CreateConnection();

                    var model = connection.CreateModel();

                    model.QueueDeclare(queueName, false, false, true, null);

                    return model;
                })
                .When(model =>
                {
                    // sender
                    var body = Encoding.UTF8.GetBytes(originalMessage);

                    model.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                })
                .Then(model =>
                {
                    // receiver
                    var consumer = new EventingBasicConsumer(model);

                    consumer.Received += (s, ea) =>
                    {
                        var body = ea.Body;
                        var receivedMessage = Encoding.UTF8.GetString(body);

                        Assert.Equal(originalMessage, receivedMessage);
                    };

                    model.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                });
        }

        [Theory]
        [InlineData("rabbitmq.test.exchange", "Hello, world!")]
        public void WhenAMessageIsPublishedOnAnExchangeThenAQueueBindedToItShouldReceiveThat(string exchange, string originalMessage)
        {
            new TestBuilder<IModel>()
                .Given(() =>
                {
                    var factory = new ConnectionFactory { HostName = "localhost" };
                    var connection = factory.CreateConnection();

                    var model = connection.CreateModel();

                    model.ExchangeDeclare(exchange: exchange, type: "fanout");

                    return model;
                })
                .When(model =>
                {
                    // sender
                    var body = Encoding.UTF8.GetBytes(originalMessage);

                    model.BasicPublish(exchange: exchange, routingKey: "", basicProperties: null, body: body);
                })
                .Then(model =>
                {
                    // receiver

                    var queueName = model.QueueDeclare().QueueName;
                    model.QueueBind(queue: queueName, exchange: exchange, routingKey: "");

                    var consumer = new EventingBasicConsumer(model);

                    consumer.Received += (s, ea) =>
                    {
                        var body = ea.Body;
                        var receivedMessage = Encoding.UTF8.GetString(body);

                        Assert.Equal(originalMessage, receivedMessage);
                    };

                    model.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
                });
        }
    }
}
