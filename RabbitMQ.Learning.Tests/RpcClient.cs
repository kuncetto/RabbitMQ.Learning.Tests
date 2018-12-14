using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Learning.Tests.Extensions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests
{
    class RpcClient : IRpcClient
    {
        private readonly EventingBasicConsumer _consumer;
        private readonly string _replyQueueName;

        public IModel Model { get; private set; }
        public string QueueName { get; private set; }

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pending =
            new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        public RpcClient(IModel model, string queueName)
        {
            Model = model;
            QueueName = queueName;

            _replyQueueName = Model.QueueDeclare().QueueName;

            _consumer = new EventingBasicConsumer(model);
            _consumer.Received += (sender, args) =>
            {
                var correlationId = args.BasicProperties.CorrelationId;
                if (!_pending.TryRemove(correlationId, out TaskCompletionSource<byte[]> tcs))
                    return;

                tcs.TrySetResult(args.Body);
            };
        }

        public Task<byte[]> CallAsync(byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
        {
            var properties = Model.CreateBasicProperties(
                correlationId: Guid.NewGuid().ToString(),
                replyTo: _replyQueueName);

            return CallAsync(properties, bytes, cancellationToken);
        }

        public Task<byte[]> CallAsync(IBasicProperties properties, byte[] bytes, CancellationToken cancellationToken =  default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<byte[]>();

            _pending.TryAdd(properties.CorrelationId, tcs);

            Model.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: properties, body: bytes);

            Model.BasicConsume(_replyQueueName, true, _consumer);

            cancellationToken.Register(() => _pending.TryRemove(properties.CorrelationId, out TaskCompletionSource<byte[]> tmp));

            return tcs.Task;
        }
    }
}
