using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Learning.Tests
{
    class RpcServerComposite : IRpcServer
    {
        private readonly IEnumerable<RpcServer> _servers;

        public RpcServerComposite(IEnumerable<RpcServer> servers)
        {
            _servers = servers ?? throw new ArgumentNullException(nameof(servers));
        }

        public void Run()
        {
            foreach (var server in _servers)
            {
                server.Run();
            }
        }
    }
}
