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

        [Fact]
        public void GivenAFactoryWhenAnUriIsAssignedThenPropertiesShouldChangeAccordingly()
        {
            new TestBuilder<ConnectionFactory>()
                .Given(() => new ConnectionFactory())
                .When(factory => factory.Uri = new Uri("amqp://username:password@hostname:1234/vhost"))
                .Then(factory =>
                {
                    Assert.Equal("username", factory.UserName);
                    Assert.Equal("password", factory.Password);
                    Assert.Equal("hostname", factory.HostName);
                    Assert.Equal(1234, factory.Port);
                    Assert.Equal("vhost", factory.VirtualHost);
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
