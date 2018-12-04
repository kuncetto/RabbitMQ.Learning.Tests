using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void WhenConnectionIsOpenThenItsStateShouldBeOpenToo()
        {
            new TestBuilder<IConnection>()
                .Given(() => new ConnectionFactory().CreateConnection())
                .Then(connection =>
                {
                    Assert.True(connection.IsOpen);
                });
        }

        [Theory]
        [InlineData("my-rabbit-client")]
        public void WhenAClientNameIsProvidedThenConnectionShouldBeSetAccordingly(string clientProvidedName)
        {
            new TestBuilder<IConnection>()
                .Given(() => new ConnectionFactory().CreateConnection(clientProvidedName))
                .Then(connection =>
                {
                    Assert.Equal(clientProvidedName, connection.ClientProvidedName);
                });
        }

        [Fact]
        public void WhenConnectionIsClosedThenItsStateShouldBeClosedToo()
        {
            new TestBuilder<IConnection>()
                .Given(() => new ConnectionFactory().CreateConnection())
                .When(connection => connection.Close())
                .Then(connection =>
                {
                    Assert.False(connection.IsOpen);
                });
        }

        [Fact]
        public void WhenConnectionIsClosedThenCreateModelShouldThrowAnException()
        {
            new TestBuilder<IConnection>()
                .Given(() => new ConnectionFactory().CreateConnection())
                .When(connection => connection.Close())
                .Then(connection =>
                {
                    Assert.Throws<AlreadyClosedException>(() => connection.CreateModel());
                });
        }

        [Fact]
        public void WhenConnectionIsOpenThenCreateModelShouldReturnAValidModel()
        {
            new TestBuilder<IConnection>()
                .Given(() => new ConnectionFactory().CreateConnection())
                .Then(connection =>
                {
                    var model = connection.CreateModel();
                    Assert.True(model.IsOpen);
                });
        }
    }
}
