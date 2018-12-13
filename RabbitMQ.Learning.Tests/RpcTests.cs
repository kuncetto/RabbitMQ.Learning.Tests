using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class RpcTests
    {
        private byte[] Encode(string s) => Encoding.UTF8.GetBytes(s);
        private string Decode(byte[] bytes) => Encoding.UTF8.GetString(bytes);

        [Theory]
        [InlineData("rabbitmq.test.rpc", "hello")]
        [InlineData("rabbitmq.test.rpc", "bye")]
        public void WhenClientCallAMethodThenServerShouldSendAResponse(string queueName, string message)
        {
            string decorate(string s) => $"***{s}***";
            byte[] decorateBytes(byte[] bytes) => Encode(decorate(Decode(bytes)));

            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            using (var client = connection.CreateModel())
            using (var server = connection.CreateModel())
            {
                new TestBuilder<RpcContext>()
                    .Given(() => new RpcContext
                    {
                        Clients = new List<IRpcClient> { new RpcClient(client, queueName) },
                        Servers = new List<IRpcServer> { new RpcServer(server, queueName, decorateBytes) }
                    })
                    .When(context => context.Servers.ForEach(s => s.Run()))
                    .Then(context =>
                    {
                        var response = context.Clients.First().CallAsync(Encode(message));
                        Assert.Equal(decorate(message), Decode(response.Result));
                    });
            }
        }

        [Theory]
        [InlineData("rabbitmq.test.rpc", "hello", "bye")]
        [InlineData("rabbitmq.test.rpc", "a", "b", "c", "d", "e")]
        public void WhenClientCallAMethodSeveralTimesThenServerShouldSendTheRespectiveResponses(string queueName, params string[] messages)
        {
            string decorate(string s) => $"***{s}***";
            byte[] decorateBytes(byte[] bytes) => Encode(decorate(Decode(bytes)));

            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            using (var client = connection.CreateModel())
            using (var server = connection.CreateModel())
            {
                new TestBuilder<RpcContext>()
                    .Given(() => new RpcContext
                    {
                        Clients = new List<IRpcClient> { new RpcClient(client, queueName) },
                        Servers = new List<IRpcServer> { new RpcServer(server, queueName, decorateBytes) }
                    })
                    .When(context => context.Servers.ForEach(s => s.Run()))
                    .Then(context =>
                    {
                        var calls = new Dictionary<string, Task<byte[]>>();

                        foreach (var message in messages)
                        {
                            calls[message] = context.Clients.First().CallAsync(Encode(message));
                        }

                        foreach (var call in calls)
                        {
                            Assert.Equal(decorate(call.Key), Decode(call.Value.Result));
                        }
                    });
            }
        }

        [Theory]
        [InlineData("rabbitmq.test.rpc", "hello")]
        public void WhenOneOrMoreClientsCallAMethodThenServerShouldSendTheRespectiveResponses(string queueName, string messageContent)
        {
            string decorate(string s) => $"***{s}***";
            byte[] decorateBytes(byte[] bytes) => Encode(decorate(Decode(bytes)));

            using (var connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            using (var client = connection.CreateModel())
            using (var server = connection.CreateModel())
            {
                new TestBuilder<RpcContext>()
                    .Given(() => new RpcContext
                    {
                        Clients = new List<IRpcClient> { new RpcClient(client, queueName), new RpcClient(client, queueName) },
                        Servers = new List<IRpcServer> { new RpcServer(server, queueName, decorateBytes) }
                    })
                    .When(context => context.Servers.ForEach(s => s.Run()))
                    .Then(context =>
                    {
                        var calls = new Dictionary<string, Task<byte[]>>();

                        context.Clients.ForEach(c => {
                            var clientId = Guid.NewGuid().ToString();
                            var message = $"Client: {clientId} Content: {messageContent}";

                            calls[message] = c.CallAsync(Encode(message));
                        });

                        foreach (var call in calls)
                        {
                            Assert.Equal(decorate(call.Key), Decode(call.Value.Result));
                        }
                    });
            }
        }

    }
}
