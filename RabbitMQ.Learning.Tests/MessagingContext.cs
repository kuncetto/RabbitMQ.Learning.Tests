using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    class MessagingContext
    {
        public IModel Sender { get; private set; }
        public IModel Receiver { get; private set; }

        public MessagingContext(Func<IModel> createSender, Func<IModel> createReceiver)
        {
            Sender = createSender.Invoke();
            Receiver = createReceiver.Invoke();
        }
    }
}
