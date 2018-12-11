using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests
{
    class RpcClient
    {
        private readonly EventingBasicConsumer _consumer;

        public IModel Model { get; private set; }
        public string QueueName { get; private set; }

        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending = 
            new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public RpcClient(IModel model, string queueName)
        {
            Model = model;
            QueueName = queueName;

            _consumer = new EventingBasicConsumer(model);
            _consumer.Received += (sender, args) =>
            {
                var correlationId = args.BasicProperties.CorrelationId;
                if (!_pending.TryRemove(correlationId, out TaskCompletionSource<string> tcs))
                    return;
                var body = args.Body;
                string response = Encoding.UTF8.GetString(body);
                tcs.TrySetResult(response);
            };
        }

        public Task<string> CallAsync(string message)
        {
            var correlationId = Guid.NewGuid().ToString();
            var replyQueueName = Model.QueueDeclare().QueueName;

            var props = Model.CreateBasicProperties();

            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            var messageBytes = Encoding.UTF8.GetBytes(message);

            var tcs = new TaskCompletionSource<string>();
            _pending.TryAdd(correlationId, tcs);

            Model.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: props, body: messageBytes);

            Model.BasicConsume(replyQueueName, true, _consumer);

            return tcs.Task;
        }
    }
}
