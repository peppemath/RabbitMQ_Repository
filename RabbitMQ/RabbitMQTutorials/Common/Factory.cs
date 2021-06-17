using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Factory
    {
        public static ConnectionFactory GetFactoryInstance()
        {
            return new ConnectionFactory()
            {
                HostName = BrokerInfo.HostName,
                UserName = BrokerInfo.UserName,
                Password = BrokerInfo.Password,
                Port = BrokerInfo.Port,
                VirtualHost = "/",
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(BrokerInfo.RequestedConnectionTimeout) // milliseconds

            };
        }
    }
}
