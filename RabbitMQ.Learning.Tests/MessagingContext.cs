using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    class MessagingContext
    {
        public static int TimeoutSeconds = 5;
        public IEnumerable<IModel> Publishers { get; private set; }
        public IEnumerable<IModel> Subscribers { get; private set; }

        public MessagingContext(Func<IModel> createPublisher, Func<IModel> createConsumer)
        {
            Publishers = new List<IModel> { createPublisher.Invoke() };
            Subscribers = new List<IModel> { createConsumer.Invoke() };
        }

        public MessagingContext(IEnumerable<IModel> publishers, IEnumerable<IModel> subscribers)
        {
            Publishers = publishers;
            Subscribers = subscribers;
        }
    }
}
