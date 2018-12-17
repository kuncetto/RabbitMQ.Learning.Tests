using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace RabbitMQ.Learning.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void OpenAConnectionUsingTheDefaultConnectionFactory()
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
        public void OpenAConnectionPassingAClientProvidedName(string clientProvidedName)
        {
            new TestBuilder<IConnection>()
                .Given(() => new ConnectionFactory().CreateConnection(clientProvidedName))
                .Then(connection =>
                {
                    Assert.Equal(clientProvidedName, connection.ClientProvidedName);
                });
        }

        [Fact]
        public void CloseAConnectionPreviouslyOpen()
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
        public void CreateAModelThrowsAnExceptionIfTheConnectionIsClosed()
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
        public void CreateAModelWhenConnectionIsOpen()
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
