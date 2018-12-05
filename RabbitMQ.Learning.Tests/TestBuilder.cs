using System;

namespace RabbitMQ.Learning.Tests
{
    class TestBuilder<TContext>
    {
        private Func<TContext> _given;
        private Action<TContext> _when;

        public TestBuilder<TContext> Given(Func<TContext> given)
        {
            _given = given;
            return this;
        }

        public TestBuilder<TContext> When(Action<TContext> when)
        {
            _when = when;
            return this;
        }

        public void Then(Action<TContext> then)
        {
            TContext context = _given.Invoke();

            _when?.Invoke(context);

            then?.Invoke(context);
        }
    }
}
