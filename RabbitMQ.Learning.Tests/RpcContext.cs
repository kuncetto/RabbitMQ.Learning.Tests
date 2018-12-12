using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    class RpcContext
    {
        public RpcClient Client { get; set; }
        public IRpcServer Server { get; set; }
    }
}
