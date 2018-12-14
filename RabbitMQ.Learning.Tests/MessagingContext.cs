﻿using System.Collections.Generic;

namespace RabbitMQ.Learning.Tests
{
    class MessagingContext
    {
        public static int TimeoutSeconds = 5;
        public IEnumerable<Publisher> Publishers { get; private set; }
        public IEnumerable<Subscriber> Subscribers { get; private set; }

        //public MessagingContext(Func<IModel> createPublisher, Func<IModel> createConsumer)
        //{
        //    Publishers = new List<IModel> { createPublisher.Invoke() };
        //    Subscribers = new List<IModel> { createConsumer.Invoke() };
        //}

        public MessagingContext(IEnumerable<Publisher> publishers, IEnumerable<Subscriber> subscribers)
        {
            Publishers = publishers;
            Subscribers = subscribers;
        }
    }
}
