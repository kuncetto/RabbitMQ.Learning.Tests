using System.Collections.Generic;

namespace RabbitMQ.Learning.Tests.Contexts.Subscription
{
    class SubscriptionContext
    {
        public IEnumerable<Publisher> Publishers { get; private set; }
        public IEnumerable<Subscriber> Subscribers { get; private set; }

        public SubscriptionContext(IEnumerable<Publisher> publishers, IEnumerable<Subscriber> subscribers)
        {
            Publishers = publishers;
            Subscribers = subscribers;
        }
    }
}
