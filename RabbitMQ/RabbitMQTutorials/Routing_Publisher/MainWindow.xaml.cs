using Common;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Routing_Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionFactory _factory;
        string _severity = BrokerInfo.Severities[0];

        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();

            txtMessage.Text = "HelloWorld " + _severity;
        }

        private void InitializeConnectionFactory()
        {
            _factory = Factory.GetFactoryInstance();
        }


        private void SendMessage(string message)
        {

            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(BrokerInfo.Exchange_Routing, ExchangeType.Direct);

                var body = Encoding.UTF8.GetBytes(message);

                //impostazione della routingkey
                channel.BasicPublish(exchange: BrokerInfo.Exchange_Routing,
                                     routingKey: _severity,
                                     basicProperties: null,
                                     body: body);

                txtLog.Text += string.Format(" [x] Sent {0}\r\n", message);
            }
        }

        Random _rnd = new Random();
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(txtMessage.Text);
            _severity = BrokerInfo.Severities[_rnd.Next(0, 3)];

            txtMessage.Text = "HelloWorld " + _severity;
        }
    }
}
