using Common;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace PublisherConfirm_Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int MESSAGE_COUNT = 50000;
        ConnectionFactory _factory;

        ConcurrentDictionary<ulong, string> _outstandingConfirms = new ConcurrentDictionary<ulong, string>();

        public MainWindow()
        {
            InitializeComponent();

            InitializeConnectionFactory();

            txtMessage.Text = "HelloWorld";
        }

        private void InitializeConnectionFactory()
        {
            _factory = Factory.GetFactoryInstance();
        }


        private void SendMessage_Individually(string message)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: BrokerInfo.QueueName_PublisherConfirm_Individually,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //per abilitare le conferme del publisher a livello di canale
                channel.ConfirmSelect();

                var timer = new Stopwatch();
                timer.Start();


                for(int i=0; i<MESSAGE_COUNT; i++)
                {
                    var body = Encoding.UTF8.GetBytes(message);

                    //publicazione sull'exchange di default
                    channel.BasicPublish(exchange: "",
                                         routingKey: BrokerInfo.QueueName_PublisherConfirm_Individually,
                                         basicProperties: null,
                                         body: body);

                    //attesa syncrona per la conferma del messaggio
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                    
                }
                
                timer.Stop();
                txtLog.Text += string.Format(" [x] Sent {0} messages individually in {1}ms\r\n", MESSAGE_COUNT, timer.ElapsedMilliseconds);
            }
        }
        private void SendMessage_InBatch(string message)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: BrokerInfo.QueueName_PublisherConfirm_InBatch,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //per abilitare le conferme del publisher a livello di canale
                channel.ConfirmSelect();

                var batchSize = 100;
                var outstandingMessageCount = 0;
                var timer = new Stopwatch();
                timer.Start();


                for (int i = 0; i < MESSAGE_COUNT; i++)
                {
                    var body = Encoding.UTF8.GetBytes(message);

                    //publicazione sull'exchange di default
                    channel.BasicPublish(exchange: "",
                                         routingKey: BrokerInfo.QueueName_PublisherConfirm_InBatch,
                                         basicProperties: null,
                                         body: body);
                    
                    outstandingMessageCount++;

                    if( outstandingMessageCount == batchSize)
                    {
                        //attesa syncrona per la conferma del messaggio
                        channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                        outstandingMessageCount = 0;
                    }
                    
                }

                timer.Stop();
                txtLog.Text += string.Format(" [x] Sent {0} messages in batch({1}) in {2}ms\r\n", MESSAGE_COUNT, batchSize, timer.ElapsedMilliseconds);
            }
        }
        private void SendMessage_Asynchronously(string message)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: BrokerInfo.QueueName_PublisherConfirm_Asynchronously,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //per abilitare le conferme del publisher a livello di canale
                channel.ConfirmSelect();

                channel.BasicAcks += Channel_BasicAcks;
                channel.BasicNacks += Channel_BasicNacks;


                var timer = new Stopwatch();
                timer.Start();

                for (int i = 0; i < MESSAGE_COUNT; i++)
                {
                    var body = i.ToString();
                    _outstandingConfirms.TryAdd(channel.NextPublishSeqNo, body);

                    //publicazione sull'exchange di default
                    channel.BasicPublish(exchange: "",
                                         routingKey: BrokerInfo.QueueName_PublisherConfirm_Asynchronously,
                                         basicProperties: null,
                                         body: Encoding.UTF8.GetBytes(body));

                    //if (!WaitUntil(60, () => _outstandingConfirms.IsEmpty))
                    //    throw new Exception("All messages could not be confirmed in 60 seconds");
                }

                timer.Stop();

                Dispatcher.Invoke(() =>
                {
                    txtLog.Text += string.Format(" [x] Sent {0} messages and handled confirm asynchronously {1}ms\r\n", MESSAGE_COUNT, timer.ElapsedMilliseconds);
                });
                
            }
        }

        private void Channel_BasicAcks(object sender, RabbitMQ.Client.Events.BasicAckEventArgs e)
        {
            CleanOutstandingConfirms(_outstandingConfirms, e.DeliveryTag, e.Multiple);           
        }

        private void Channel_BasicNacks(object sender, RabbitMQ.Client.Events.BasicNackEventArgs e)
        {
            _outstandingConfirms.TryGetValue(e.DeliveryTag, out string body);

            Dispatcher.Invoke(() =>
            {
                txtLog.Text += string.Format(" [x] Message with body {0} has been nack-ed. Sequence number: {1}, multiple: {2}\r\n", body, e.DeliveryTag, e.Multiple);
            });

            CleanOutstandingConfirms(_outstandingConfirms, e.DeliveryTag, e.Multiple);
        }

        

        void CleanOutstandingConfirms(ConcurrentDictionary<ulong, string> outstandingConfirms, ulong sequenceNumber, bool multiple)
        {
            if (multiple)
            {
                var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                foreach (var entry in confirmed)
                    outstandingConfirms.TryRemove(entry.Key, out _);
            }
            else
                outstandingConfirms.TryRemove(sequenceNumber, out _);
        }

        private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
        {
            int waited = 0;
            while (!condition() && waited < numberOfSeconds * 1000)
            {
                Thread.Sleep(100);
                waited += 100;
            }

            return condition();
        }


        private void btnSendIndividually_Click(object sender, RoutedEventArgs e)
        {
            SendMessage_Individually(txtMessage.Text);

            txtMessage.Text = "Individually";
        }
        private void btnSendInBatch_Click(object sender, RoutedEventArgs e)
        {
            SendMessage_InBatch(txtMessage.Text);

            txtMessage.Text = "InBatch";
        }
        private void btnSendAsynchronously_Click(object sender, RoutedEventArgs e)
        {
            var msg = txtMessage.Text;
            var thread = new Thread(() =>
            {
                SendMessage_Asynchronously(msg);
            });
            thread.Start();

            txtMessage.Text = "Asynchronously";
        }

    }
}
