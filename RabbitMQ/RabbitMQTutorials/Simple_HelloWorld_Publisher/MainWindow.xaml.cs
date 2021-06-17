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

namespace Simple_HelloWorld_Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionFactory _factory;

        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();
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
                channel.QueueDeclare(queue: BrokerInfo.QueueName_Simple_HelloWorld,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                //Il primo parametro è il nome dello scambio. La stringa vuota denota lo scambio predefinito o senza nome :
                //i messaggi vengono instradati alla coda con il nome specificato da routingKey , se esiste.
                channel.BasicPublish(exchange: "",
                                     routingKey: BrokerInfo.QueueName_Simple_HelloWorld,
                                     basicProperties: null,
                                     body: body);

                txtLog.Text += string.Format(" [x] Sent {0}\r\n", message);
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(txtMessage.Text);
        }
    }
}
