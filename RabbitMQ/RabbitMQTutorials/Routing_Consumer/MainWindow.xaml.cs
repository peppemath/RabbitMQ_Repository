﻿using Common;
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

namespace Routing_Consumer
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
        string _queueName;
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

            _channel.ExchangeDeclare(BrokerInfo.Exchange_Routing, ExchangeType.Direct);

            //Nel client .NET, quando non forniamo parametri a QueueDeclare()
            //creiamo una coda di eliminazione automatica non durevole, esclusiva con un nome generato automaticamente
            //cioè durable:false, exclusive:true, autodelete:true, nome del tipo amq.gen-JzTY20BRgKO-HjmUJj0wLg
            _queueName = _channel.QueueDeclare().QueueName;

            txtTempQueue.Text = string.Format("Tempoaray queue={0}", _queueName);

            
        }


        private void start()
        {
            btnStartReceiving.IsEnabled = false;
            chkInfo.IsEnabled = false;
            chkWarning.IsEnabled = false;
            chkError.IsEnabled = false;

            //Ora dobbiamo dire all'exchange di inviare messaggi alla nostra coda ed impostare la routingkey
            if (chkInfo.IsChecked.Value)
            {
                _channel.QueueBind(queue: _queueName,
                                exchange: BrokerInfo.Exchange_Routing,
                                routingKey: BrokerInfo.Severities[0]);
            }
            if (chkWarning.IsChecked.Value)
            {
                _channel.QueueBind(queue: _queueName,
                                exchange: BrokerInfo.Exchange_Routing,
                                routingKey: BrokerInfo.Severities[1]);
            }
            if (chkError.IsChecked.Value)
            {
                _channel.QueueBind(queue: _queueName,
                                exchange: BrokerInfo.Exchange_Routing,
                                routingKey: BrokerInfo.Severities[2]);
            }


            _consumer = new EventingBasicConsumer(_channel);




            _tag_CONSUMER = _channel.BasicConsume(queue: _queueName,
                                                 autoAck: true,
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
            var routingKey = e.RoutingKey;

            Dispatcher.Invoke(() =>
            {
                txtLog.Text += string.Format(" [x] Received {0}:{1}\r\n", routingKey, message);
            });
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
