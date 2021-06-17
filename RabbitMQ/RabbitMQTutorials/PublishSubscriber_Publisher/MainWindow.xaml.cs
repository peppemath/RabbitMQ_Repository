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

namespace PublishSubscriber_Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionFactory _factory;
        int _counter = 1;

        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();

            txtMessage.Text = "HelloWorld" + _counter.ToString();
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
                //creazione dell'exchange che è di tipo Fanout: trasmette semplicemente tutti i messaggi che riceve a TUTTE le code che conosce
                channel.ExchangeDeclare(BrokerInfo.Exchange_PublishSubscriber, ExchangeType.Fanout);               

                var body = Encoding.UTF8.GetBytes(message);
            
                //publicazione sull'exchange NON di default
                channel.BasicPublish(exchange: BrokerInfo.Exchange_PublishSubscriber,
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);

                txtLog.Text += string.Format(" [x] Sent {0}\r\n", message);
            }
        }


        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(txtMessage.Text);
            _counter++;
            txtMessage.Text = "HelloWorld" + _counter.ToString();
        }


    }
}
