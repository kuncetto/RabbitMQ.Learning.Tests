using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests.Contexts.Subscription
{
    class Subscriber
    {
        private readonly EventingBasicConsumer _consumer;

        public IModel Model { get; private set; }
        public string QueueName { get; private set; }

        public Subscriber(IModel model, string queue, string exchange, string routingKey)
        {
            Model = model;
            QueueName = queue;

            Model.QueueDeclare(queue, false, false, true, null);

            if (!string.IsNullOrEmpty(exchange))
            {
                Model.ExchangeDeclare(exchange: exchange, type: "fanout");
                Model.QueueBind(queue: queue, exchange: exchange, routingKey: routingKey);
            }

            _consumer = new EventingBasicConsumer(Model);
        }

        public Task<byte[]> ConsumeAsync()
        {
            var tcs = new TaskCompletionSource<byte[]>();

            void onReceived(object sender, BasicDeliverEventArgs args)
            {
                _consumer.Received -= onReceived;

                tcs.TrySetResult(args.Body);
            }

            _consumer.Received += onReceived;

            Model.BasicConsume(QueueName, true, _consumer);

            return tcs.Task;
        }

    }
}
