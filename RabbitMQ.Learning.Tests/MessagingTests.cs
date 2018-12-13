using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class MessagingTests
    {

        public Func<byte[], byte[]> RemoteProcedure => 
            (b) => Encoding.UTF8.GetBytes(ServerReply(Encoding.UTF8.GetString(b)));

        public Func<string, string> ServerReply => 
            (s) => $"remote: {s}";


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
                            () => connection.CreateModel().ForConsumer(exchange, routingKey, queue));
                    })
                    .When(context =>
                    {
                        context.Publisher.Publish(exchange: exchange, routingKey: routingKey, message: message);
                    })
                    .Then(context =>
                    {
                        context.Consumer.ConsumeWithTimeout(
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
                            () => connection.CreateModel().ForPublisher(exchange, routingKey, queue),
                            () => connection.CreateModel().ForConsumer(exchange, routingKey, queue));
                    })
                    .When(context =>
                    {
                        context.Publisher.Publish(exchange: exchange, routingKey: routingKey, message: message);
                    })
                    .Then(context =>
                    {
                        var received = context.Consumer.ConsumeAsync(queue: queue).Result;
                        Assert.Equal(message, received);
                    });
            }
        }

        [Theory]
        [InlineData("rabbitmq.test.rpc", "hello")]
        [InlineData("rabbitmq.test.rpc", "bye")]
        public void WhenClientCallAMethodThenServerShouldSendAResponse(string queueName, string message)
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            using (var client = connection.CreateModel())
            using (var server = connection.CreateModel())
            {
                new TestBuilder<RpcContext>()
                    .Given(() => new RpcContext
                    {
                        Client = new RpcClient(client, queueName),
                        Server = new RpcServer(server, queueName, RemoteProcedure)
                    })
                    .When(context => context.Server.Run())
                    .Then(context =>
                    {
                        var response = context.Client.CallAsync(Encoding.UTF8.GetBytes(message));
                        Assert.Equal(ServerReply(message), Encoding.UTF8.GetString(response.Result));
                    });
            }
        }

        [Theory]
        [InlineData("rabbitmq.test.rpc", "hello", "bye")]
        [InlineData("rabbitmq.test.rpc", "a", "b", "c", "d", "e")]
        public void WhenClientCallAMethodSeveralTimesThenServerShouldSendTheRespectiveResponses(string queueName, params string[] messages)
        {
            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            using (var client = connection.CreateModel())
            using (var server = connection.CreateModel())
            {
                new TestBuilder<RpcContext>()
                    .Given(() => new RpcContext
                    {
                        Client = new RpcClient(client, queueName),
                        Server = new RpcServer(server, queueName, RemoteProcedure)
                    })
                    .When(context => context.Server.Run())
                    .Then(context =>
                    {
                        var calls = new Dictionary<string, Task<byte[]>>();

                        foreach (var message in messages)
                        {
                            calls[message] = context.Client.CallAsync(Encoding.UTF8.GetBytes(message));
                        }

                        foreach (var call in calls)
                        {
                            Assert.Equal(ServerReply(call.Key), Encoding.UTF8.GetString(call.Value.Result));
                        }
                    });
            }
        }

        //[Theory]
        //[InlineData("rabbitmq.test.rpc.1", "rabbitmq.test.rpc.2", "hello")]
        //[InlineData("rabbitmq.test.rpc.1", "rabbitmq.test.rpc.2", "bye")]
        //public void WhenClientCallTwoMethodsThenServersShouldSendTheirRespectiveResponses(string firstQueueName, string secondQueueName, string message)
        //{
        //    using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
        //    using (var client = connection.CreateModel())
        //    using (var server = connection.CreateModel())
        //    {
        //        new TestBuilder<RpcContext>()
        //            .Given(() => new RpcContext
        //            {
        //                Client = new RpcClient(client, firstQueueName),
        //                Server = new RpcServerComposite(
        //                    new List<RpcServer> {
        //                        new RpcServer(server, firstQueueName, (s) => $"{firstQueueName}>> " + s),
        //                        new RpcServer(server, secondQueueName, (s) => $"{secondQueueName}>> " + s),
        //                    })
        //            })
        //            .When(context => context.Server.Run())
        //            .Then(context =>
        //            {
        //                var firstCall = context.Client.CallAsync(firstQueueName, message);
        //                var secondCall = context.Client.CallAsync(secondQueueName, message);
        //                Assert.Equal($"{firstQueueName}>> {message}", firstCall.Result);
        //                Assert.Equal($"{secondQueueName}>> {message}", secondCall.Result);
        //            });
        //    }
        //}
    }
}
