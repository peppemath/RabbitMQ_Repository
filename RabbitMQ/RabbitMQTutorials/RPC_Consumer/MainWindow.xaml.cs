using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

namespace RPC_Consumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionFactory _factory;

        IConnection _connection;
        IModel _channel;
        EventingBasicConsumer _consumer;
        string _tag_CONSUMER;

        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();

            InitializeReceiver();
        }

        private void InitializeConnectionFactory()
        {
            _factory = Factory.GetFactoryInstance();
        }

        private void InitializeReceiver()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

           
            try
            {
                _channel.QueueDeclare(queue: BrokerInfo.QueueName_RPC,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                _channel.BasicQos(0, 1, false);
            }
            catch (Exception ex)
            {
                //vedi il commento precedente ...RabbitMQ non ti consente di ridefinire una coda esistente ....

                var msg = string.Format("{0}\r\n{1}", ex.Message, "...RabbitMQ non ti consente di ridefinire una coda esistente ....");
                MessageBox.Show(msg);

                Application.Current.Shutdown();
            }


            _consumer = new EventingBasicConsumer(_channel);
        }


        private void start()
        {
            btnStartReceiving.IsEnabled = false;

            //conferma manuale per non perdere i messaggi autoAck:false
            _tag_CONSUMER = _channel.BasicConsume(queue: BrokerInfo.QueueName_RPC,
                                                 autoAck: false,
                                                 consumer: _consumer);

            _consumer.Received += _consumer_Received;
        }
        private void btnStartReceiving_Click(object sender, RoutedEventArgs e)
        {
            start();
        }

        private void _consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            string response = null;

            var body = e.Body.ToArray();
            var props = e.BasicProperties;
            
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var message = Encoding.UTF8.GetString(body);

                int n = int.Parse(message);

                Dispatcher.Invoke(() =>
                {
                    txtLog.Text += string.Format(" [x] Fibonacci({0}) ....", message);
                });

                response = CalculateFibonacci(n).ToString();
            }
            catch(Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    txtLog.Text += string.Format(" [x] errore:", ex.Message);
                });

                response = "";
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);
                
                _channel.BasicPublish(exchange: "", 
                                        routingKey: props.ReplyTo,
                                        basicProperties: replyProps,
                                        body: responseBytes);

                //conferma manuale per non perdere i messaggi
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

                Dispatcher.Invoke(() =>
                {
                    txtLog.Text += string.Format(" OK\r\n", response);
                });
            }
      
        }


        private void stop()
        {
            _consumer.Received -= _consumer_Received;
            _channel.BasicCancel(_tag_CONSUMER);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stop();

            _channel.Close();
            _connection.Close();
        }


        private static int CalculateFibonacci(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return CalculateFibonacci(n - 1) + CalculateFibonacci(n - 2);
        }
    }
}
