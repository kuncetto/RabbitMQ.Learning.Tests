using RabbitMQ.Client;
using System;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class ConnectionFactoryTests
    {
        [Fact]
        public void GivenAFactoryWhenNoUriIsProvidedThenPropertiesShouldBeAsDefault()
        {
            var factory = new ConnectionFactory();

            Assert.Equal("guest", factory.UserName);
            Assert.Equal("guest", factory.Password);
            Assert.Equal("localhost", factory.HostName);
            Assert.Equal(-1, factory.Port);
            Assert.Equal("/", factory.VirtualHost);
        }

        [Fact]
        public void GivenAFactoryWhenAnUriIsAssignedThenPropertiesShouldChangeAccordingly()
        {
            var factory = new ConnectionFactory();

            factory.Uri = new Uri("amqp://username:password@hostname:1234/vhost");

            Assert.Equal("username", factory.UserName);
            Assert.Equal("password", factory.Password);
            Assert.Equal("hostname", factory.HostName);
            Assert.Equal(1234, factory.Port);
            Assert.Equal("vhost", factory.VirtualHost);
        }
    }
}
