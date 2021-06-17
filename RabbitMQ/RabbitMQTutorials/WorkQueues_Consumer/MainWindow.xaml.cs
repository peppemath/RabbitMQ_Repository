using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
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

namespace WorkQueues_Consumer
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

            /*
             * Quando RabbitMQ si chiude o si arresta in modo anomalo, dimenticherà le code e i messaggi a meno che tu non gli dica di non farlo. 
             * Sono necessarie due cose per assicurarsi che i messaggi non vadano persi: è necessario contrassegnare sia la coda che i messaggi 
             * come durevoli.
             * Innanzitutto, dobbiamo assicurarci che la coda sopravviva a un riavvio del nodo RabbitMQ. 
             * Per fare ciò, dobbiamo dichiararlo come durevole 
             * 
             * Sebbene questo comando sia corretto da solo, non funzionerà nella nostra configurazione attuale. 
             * Questo perché abbiamo già definito una coda chiamata ciao che non è durevole. RabbitMQ non ti consente 
             * di ridefinire una coda esistente con parametri diversi e restituirà un errore a qualsiasi programma che tenti di farlo. 
             * Ma c'è una soluzione rapida: dichiariamo una coda con un nome diverso, ad esempio task_queue 
             * 
             * Questa modifica QueueDeclare deve essere applicata sia al codice produttore che al codice consumatore.
             * 
             * A questo punto siamo sicuri che la coda task_queue non andrà persa anche se RabbitMQ si riavvia. 
             * Ora dobbiamo contrassegnare i nostri messaggi come persistenti, impostando IBasicProperties.SetPersistent su true .
            */
            try
            {
                _channel.QueueDeclare(queue: BrokerInfo.QueueName_WorkQueues,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
            }
            catch(Exception ex)
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
            _tag_CONSUMER = _channel.BasicConsume(queue: BrokerInfo.QueueName_WorkQueues,
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
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Dispatcher.Invoke(() =>
            {
                txtLog.Text += string.Format(" [x] Received {0} ....", message);
            });


            Random rnd = new Random();
            Thread.Sleep(rnd.Next(1,10)*1000);

            Dispatcher.Invoke(() =>
            {
                txtLog.Text += string.Format(" OK\r\n", message);
            });

            //conferma manuale per non perdere i messaggi
            _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
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
