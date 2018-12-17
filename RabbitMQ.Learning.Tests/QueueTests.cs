using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Learning.Tests.Extensions;
using System;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class QueueTests
    {
        [Fact]
        public void DeclareAQueueWithAValidName()
        {
            var queueName = "rabbitmq.test.my_queue";
            DeclareAQueueSuccessfully(queueName, queueName);
        }

        [Fact]
        public void DeclareAServerNamedQueue()
        {
            var queueName = string.Empty;
            DeclareAQueueSuccessfully(queueName, "amq.gen");
        }

        [Fact]
        public void DeclareAQueueWithReservedAmqPrefixThrowsAnException()
        {
            var queueName = "amq.test.my_queue";
            DeclareAQueueThrowsAnException<OperationInterruptedException>(queueName);
        }

        [Fact]
        public void DeclareAQueueWithNameLongerThan255CharactersThrowsAnException()
        {
            var queueName = new Random().NextString(length: 256);
            DeclareAQueueThrowsAnException<WireFormattingException>(queueName);
        }

        private void DeclareAQueueSuccessfully(string queueName, string expectedQueueName)
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            {
                new TestBuilder<IModel>()
                    .Given(() => connection.CreateModel())
                    .Then(context =>
                    {
                        var actual = context.QueueDeclare(queue: queueName);
                        Assert.StartsWith(expectedQueueName, actual.QueueName);
                    });
            }
        }

        private void DeclareAQueueThrowsAnException<TException>(string queueName) where TException : Exception
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            {
                new TestBuilder<IModel>()
                    .Given(() => connection.CreateModel())
                    .Then(context =>
                    {
                        Assert.Throws<TException>(() => context.QueueDeclare(queue: queueName));
                    });
            }
        }
    }
}
