using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests.Contexts.Rpc
{
    class RpcContext
    {
        public IEnumerable<IRpcClient> Clients { get; set; }
        public IEnumerable<IRpcServer> Servers { get; set; }
    }
}
