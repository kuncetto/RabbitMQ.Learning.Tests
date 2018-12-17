using System;
using System.Text;

namespace RabbitMQ.Learning.Tests.Extensions
{
    static class RandomExtensions
    {
        public static string NextString(this Random random, int length)
        {
            const int MaxAsciiDecimalValue = 126;
            var sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                var n = random.Next(MaxAsciiDecimalValue);
                sb.Append(Convert.ToChar(n));
            }

            return sb.ToString();
        }
    }
}
