using System;
using System.Collections.Generic;

namespace RabbitMQ.Learning.Tests.Extensions
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
