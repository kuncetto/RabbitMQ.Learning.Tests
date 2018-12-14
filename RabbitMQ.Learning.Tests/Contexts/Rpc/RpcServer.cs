using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Learning.Tests.Extensions;
using System;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests.Contexts.Rpc
{
    class RpcServer : IRpcServer
    {
        private readonly IModel _model;
        private readonly string _queueName;
        private readonly EventingBasicConsumer _consumer;

        public RpcServer(IModel model, string queueName, Func<byte[], byte[]> procedure)
        {
            _model = model;
            _queueName = queueName;

            _model.QueueDeclare(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _model.BasicQos(0, 1, false);

            _consumer = new EventingBasicConsumer(_model);
            _consumer.Received += async (sender, args) =>
            {
                await Task.Factory.StartNew(() => {
                    var body = args.Body;
                    var props = args.BasicProperties;

                    var replyProps = _model.CreateBasicProperties(correlationId: props.CorrelationId);

                    var reply = procedure.Invoke(args.Body);

                    _model.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: reply);
                });               
            };
        }

        public void Run()
        {
            if (_consumer.IsRunning) return;

            _model.BasicConsume(_queueName, autoAck: true, consumer: _consumer);
        }
    }
}
