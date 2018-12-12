using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests
{
    class RpcClient
    {
        private readonly EventingBasicConsumer _consumer;

        public IModel Model { get; private set; }
        public string QueueName { get; private set; }

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pending =
            new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        public RpcClient(IModel model, string queueName)
        {
            Model = model;
            QueueName = queueName;

            _consumer = new EventingBasicConsumer(model);
            _consumer.Received += (sender, args) =>
            {
                var correlationId = args.BasicProperties.CorrelationId;
                if (!_pending.TryRemove(correlationId, out TaskCompletionSource<byte[]> tcs))
                    return;

                tcs.TrySetResult(args.Body);
            };
        }

        public Task<byte[]> CallAsync(byte[] bytes)
        {
            var replyTo = Model.QueueDeclare().QueueName;

            var properties = Model.CreateBasicProperties(
                correlationId: Guid.NewGuid().ToString(),
                replyTo: replyTo);

            return CallAsync(properties, bytes);
        }

        public Task<byte[]> CallAsync(IBasicProperties properties, byte[] bytes)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            _pending.TryAdd(properties.CorrelationId, tcs);

            Model.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: properties, body: bytes);

            Model.BasicConsume(properties.ReplyTo, true, _consumer);

            return tcs.Task;
        }
    }
}
