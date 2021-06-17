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

namespace Topics_Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionFactory _factory;
        string _topics;

        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();

            _topics = GenerateTopics();
            txtMessage.Text = "HelloWorld " + _topics;
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
                channel.ExchangeDeclare(BrokerInfo.Exchange_Topics, ExchangeType.Topic);

                var body = Encoding.UTF8.GetBytes(message);

                //impostazione della routingkey
                channel.BasicPublish(exchange: BrokerInfo.Exchange_Topics,
                                     routingKey: _topics,
                                     basicProperties: null,
                                     body: body);

                txtLog.Text += string.Format(" [x] Sent {0}\r\n", message);
            }
        }

        Random _rnd = new Random();
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(txtMessage.Text);
            _topics = GenerateTopics();

            txtMessage.Text = "HelloWorld " + _topics;
        }

        private string GenerateTopics()
        {
            var topics = new List<string>() {
                "quick.orange.rabbit",
                "lazy.orange.elephant",
                "quick.orange.fox",
                "lazy.brown.fox",
                "lazy.pink.rabbit",
                "quick.brown.fox",
                "quick.orange.male.rabbit",
                "lazy.orange.male.rabbit" };
            return topics[_rnd.Next(0, 8)];
        }
    }
}
