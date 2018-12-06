using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    class MessagingContext
    {
        public static int TimeoutSeconds = 5;
        public IModel Publisher { get; private set; }
        public IModel Consumer { get; private set; }

        public MessagingContext(Func<IModel> createPublisher, Func<IModel> createConsumer)
        {
            Publisher = createPublisher.Invoke();
            Consumer = createConsumer.Invoke();
        }
    }
}
