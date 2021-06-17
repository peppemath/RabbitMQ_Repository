using System;
using System.Collections.Generic;

namespace Common
{
    public static class BrokerInfo
    {
        public const string HostName = "localhost";
        public const string UserName = "guest";
        public const string Password = "guest";
        public const int Port = 5672;
        public const int RequestedConnectionTimeout = 3000; // milliseconds


        public const string QueueName_Simple_HelloWorld = "hello_queue";
        public const string QueueName_WorkQueues = "task_queue";
        public const string QueueName_RPC = "rpc_queue";
        public const string QueueName_PublisherConfirm_Individually = "PublisherConfirm_Individually_queue";
        public const string QueueName_PublisherConfirm_InBatch = "PublisherConfirm_InBatch_queue";
        public const string QueueName_PublisherConfirm_Asynchronously = "PublisherConfirm_Asynchronously_queue";


        public const string Exchange_PublishSubscriber = "logs_exchange";
        public const string Exchange_Routing = "routing_logs_exchange";
        public const string Exchange_Topics = "topics_logs_exchange";
        public const string Exchange_PublishConfirm = "publishconfirm_logs_exchange";


        public static readonly List<string> Severities = new List<string>() { "info", "warning", "error" };


        public static readonly List<string> Speed = new List<string>() { "*", "#", "lazy", "quick" };
        public static readonly List<string> Colors = new List<string>() { "*", "#", "brown", "pink", "orange" };
        public static readonly List<string> Animals = new List<string>() { "*", "#", "rabbit", "elephant", "fox" };


        public static List<string> NavigateUrl = new List<string>()
        {
            "https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html",
            "https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html",
            "https://www.rabbitmq.com/tutorials/tutorial-three-dotnet.html",
            "https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html",
            "https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html",
            "https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html",
            "https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet.html"
        };
    }
}
