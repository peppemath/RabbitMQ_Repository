using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common
{
    public class BaseWindow : Window
    {
        ConnectionFactory _factory;

        IConnection _connection;
        IModel _channel;
        EventingBasicConsumer _consumer;
        string _tag_CONSUMER;
    }
}
