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

namespace WorkQueues_Publisher
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
                channel.QueueDeclare(queue: BrokerInfo.QueueName_WorkQueues,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                //Il primo parametro è il nome dello scambio. La stringa vuota denota lo scambio predefinito o senza nome :
                //i messaggi vengono instradati alla coda con il nome specificato da routingKey , se esiste.
                channel.BasicPublish(exchange: "",
                                     routingKey: BrokerInfo.QueueName_WorkQueues,
                                     basicProperties: properties,
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
