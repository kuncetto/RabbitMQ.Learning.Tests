using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests
{
    static class IModelExtensions
    {
        public static IModel DeclareQueue(this IModel model, string queue)
        {
            model.QueueDeclare(queue, false, false, true, null);

            return model;
        }

        public static IModel DeclareExchange(this IModel model, string exchange)
        {
            if(!string.IsNullOrEmpty(exchange))
            {
                model.ExchangeDeclare(exchange: exchange, type: "fanout");
            }

            return model;
        }

        public static IModel Bind(this IModel model, string queue, string exchange, string routingKey)
        {
            if (!string.IsNullOrEmpty(exchange))
            {
                model.QueueBind(queue: queue, exchange: exchange, routingKey: routingKey);
            }

            return model;
        }

        public static IModel ForPublisher(this IModel model, string exchange, string routingKey, string queue)
        {
            return model.DeclareQueue(queue).DeclareExchange(exchange);
        }

        public static IModel ForConsumer(this IModel model, string exchange, string routingKey, string queue)
        {
            return model.DeclareQueue(queue).DeclareExchange(exchange).Bind(queue, exchange, routingKey);
        }

        public static void Publish(this IModel model, string exchange, string routingKey, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            model.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
        }

        public static void ConsumeWithTimeout(this IModel model, string queue, TimeSpan timeout, 
            Action<string> onReceived, Action<Exception> onError = null, Action<TimeSpan> onTimeout = null)
        {
            var resetEvent = new AutoResetEvent(false);
            var consumer = new EventingBasicConsumer(model);

            consumer.Received += (s, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var receivedMessage = Encoding.UTF8.GetString(body);

                    onReceived(receivedMessage);

                    model.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    model.BasicNack(ea.DeliveryTag, false, false);
                    onError?.Invoke(ex);
                }
                finally
                {
                    resetEvent.Set();
                }
            };

            model.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            
            if (!resetEvent.WaitOne(timeout))
            {
                // timeout
                onTimeout?.Invoke(timeout);
            }
        }

        public static async Task<string> ConsumeAsync(this IModel model, string queue)
        {
            var message = string.Empty;
            var tcs = new TaskCompletionSource<string>();

            EventHandler<BasicDeliverEventArgs> onReceived = (s, e) =>
            {
                var body = e.Body;
                var receivedMessage = Encoding.UTF8.GetString(body);

                tcs.TrySetResult(receivedMessage);
            };

            EventHandler<ConsumerEventArgs> onCancelled = (s, e) => tcs.TrySetCanceled();

            var consumer = new EventingBasicConsumer(model);

            try
            {
                consumer.Received += onReceived;
                consumer.ConsumerCancelled += onCancelled;
                model.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
                message = await tcs.Task;
            }
            finally
            {
                consumer.Received -= onReceived;
                consumer.ConsumerCancelled -= onCancelled;
            }

            return message;
        }

        public static IBasicProperties CreateBasicProperties(this IModel model, string correlationId, string replyTo)
        {
            var properties = model.CreateBasicProperties();

            properties.CorrelationId = correlationId;
            properties.ReplyTo = replyTo;

            return properties;
        }
    }
}
