using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable)
            {
                action(e);
            }
        }
    }
}
