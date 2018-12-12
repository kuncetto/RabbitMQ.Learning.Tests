using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    class RpcServer : IRpcServer
    {
        private readonly IModel _model;
        private readonly string _queueName;
        private readonly EventingBasicConsumer _consumer;

        public RpcServer(IModel model, string queueName, Func<string, string> procedure)
        {
            _model = model;
            _queueName = queueName;

            _model.QueueDeclare(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _model.BasicQos(0, 1, false);

            _consumer = new EventingBasicConsumer(_model);
            _consumer.Received += (sender, args) =>
            {
                var body = args.Body;
                var props = args.BasicProperties;

                var replyProps = _model.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                var message = Encoding.UTF8.GetString(body);

                var response = procedure.Invoke(message);

                var responseBytes = Encoding.UTF8.GetBytes(response);

                _model.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
            };
        }

        public void Run()
        {
            _model.BasicConsume(_queueName, autoAck: true, consumer: _consumer);
        }
    }
}
