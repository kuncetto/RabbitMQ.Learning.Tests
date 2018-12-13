﻿using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Learning.Tests
{
    interface IRpcClient
    {
        string QueueName { get; }

        Task<byte[]> CallAsync(byte[] bytes, CancellationToken cancellationToken = default(CancellationToken));
    }
}