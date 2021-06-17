using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
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

namespace RPC_Publisher
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

        private string _replyQueueName;
        //private BlockingCollection<string> _respQueue = new BlockingCollection<string>();
        private IBasicProperties _props;
        private string _correlationId;

        private int _number = 0;
        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();

            txtMessage.Text = "Fibonacci of " + _number.ToString();

            InitializeClient();
        }

        private void InitializeConnectionFactory()
        {
            _factory = Factory.GetFactoryInstance();
        }

        private void InitializeClient()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();


            try
            {
                _replyQueueName = _channel.QueueDeclare().QueueName;
                _correlationId = Guid.NewGuid().ToString();

                _props = _channel.CreateBasicProperties();
                _props.CorrelationId = _correlationId;
                _props.ReplyTo = _replyQueueName;

                _consumer = new EventingBasicConsumer(_channel);

                start();
            }
            catch (Exception ex)
            {
                //vedi il commento precedente ...RabbitMQ non ti consente di ridefinire una coda esistente ....

                var msg = string.Format("{0}\r\n{1}", ex.Message, "...RabbitMQ non ti consente di ridefinire una coda esistente ....");
                MessageBox.Show(msg);

                Application.Current.Shutdown();
            }

            
        }

        private void start()
        {
            //btnStartReceiving.IsEnabled = false;

            //conferma manuale per non perdere i messaggi autoAck:false
            _tag_CONSUMER = _channel.BasicConsume(consumer: _consumer,
                                                    queue: _replyQueueName,
                                                    autoAck: true);

            _consumer.Received += _consumer_Received;
        }


        private void _consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var props = e.BasicProperties;

            try
            {
                var message = Encoding.UTF8.GetString(body);

                if( e.BasicProperties.CorrelationId == _correlationId)
                {
                    //_respQueue.Add(message);

                    Dispatcher.Invoke(() =>
                    {
                        txtLog.Text += string.Format(" [x] calc={0}\r\n", message);
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        txtLog.Text += string.Format(" discarded\r\n");
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    txtLog.Text += string.Format(" [x] errore:", ex.Message);
                });
            }

        }


        //private void SendMessage(string message)
        //{

        //    using (var connection = _factory.CreateConnection())
        //    using (var channel = connection.CreateModel())
        //    {
                
        //        channel.QueueDeclare(queue: BrokerInfo.QueueName_WorkQueues,
        //                             durable: true,
        //                             exclusive: false,
        //                             autoDelete: false,
        //                             arguments: null);

        //        var body = Encoding.UTF8.GetBytes(message);

        //        var properties = channel.CreateBasicProperties();
        //        properties.Persistent = true;

        //        //Il primo parametro è il nome dello scambio. La stringa vuota denota lo scambio predefinito o senza nome :
        //        //i messaggi vengono instradati alla coda con il nome specificato da routingKey , se esiste.
        //        channel.BasicPublish(exchange: "",
        //                             routingKey: BrokerInfo.QueueName_WorkQueues,
        //                             basicProperties: properties,
        //                             body: body);

        //        txtLog.Text += string.Format(" [x] Sent {0}\r\n", message);
        //    }
        //}

        private string SendMessage(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                                    routingKey: BrokerInfo.QueueName_RPC,
                                    basicProperties: _props,
                                    body: messageBytes);

            txtLog.Text += string.Format(" [x] Sent {0}\r\n", message);

            return "";
            //return _respQueue.Take();
        }


        Random _rnd = new Random();
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(_number.ToString());

            _number = _rnd.Next(0, 30);
            txtMessage.Text = "Fibonacci of " + _number.ToString();
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

    }
}
