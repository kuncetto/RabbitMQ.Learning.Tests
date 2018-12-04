using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class ConnectionFactoryTests
    {
        [Fact]
        public void GivenAFactoryThenPropertiesShouldBeAsDefault()
        {
            new TestBuilder<ConnectionFactory>()
                .Given(() => new ConnectionFactory())
                .Then(factory =>
                {
                    Assert.Equal("guest", factory.UserName);
                    Assert.Equal("guest", factory.Password);
                    Assert.Equal("localhost", factory.HostName);
                    Assert.Equal(-1, factory.Port);
                    Assert.Equal("/", factory.VirtualHost);
                });
        }

        [Theory]
        [InlineData("amqp://username:password@hostname:1234/vhost", "username", "password", "hostname", 1234, "vhost")]
        [InlineData("amqp://hostname:1234/vhost", "guest", "guest", "hostname", 1234, "vhost")]
        [InlineData("amqp://hostname:1234", "guest", "guest", "hostname", 1234, "/")]
        public void GivenAFactoryWhenAValidUriIsAssignedThenPropertiesShouldChangeAccordingly(string uri, params object[] expected)
        {
            new TestBuilder<ConnectionFactory>()
                .Given(() => new ConnectionFactory())
                .When(factory => factory.Uri = new Uri(uri))
                .Then(factory =>
                {
                    Assert.Equal(expected[0], factory.UserName);
                    Assert.Equal(expected[1], factory.Password);
                    Assert.Equal(expected[2], factory.HostName);
                    Assert.Equal(expected[3], factory.Port);
                    Assert.Equal(expected[4], factory.VirtualHost);
                });
        }

        [Fact]
        public void GivenAFactoryWhenHostIsUnreachableThenCreateConnectionShouldThrownAnException()
        {
            new TestBuilder<ConnectionFactory>()
                .Given(() => new ConnectionFactory())
                .When(factory => factory.HostName = "unknown-host")
                .Then(factory =>
                {
                    Assert.Throws<BrokerUnreachableException>(() => factory.CreateConnection());
                });
        }

        [Fact]
        public void GivenAFactoryWhenHostIsReachableThenCreateConnectionShouldReturnAValidConnection()
        {
            new TestBuilder<ConnectionFactory>()
                .Given(() => new ConnectionFactory())
                .When(factory => factory.HostName = "localhost")
                .Then(factory =>
                {
                    var connection = factory.CreateConnection();

                    Assert.NotNull(connection);
                    Assert.True(connection.IsOpen);
                });
        }

    }
}
