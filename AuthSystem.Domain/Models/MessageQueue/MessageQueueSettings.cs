using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models.MessageQueue
{
    public class MessageQueueSettings
    {
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public int RetryCount { get; set; }
        public int RetryInterval { get; set; }
        public int PrefetchCount { get; set; }
        public int ConcurrentMessageLimit { get; set; }
        public QueueNames QueueNames { get; set; }
    }

    public class QueueNames
    {
        public string EmailQueue { get; set; }
        public string ReportQueue { get; set; }
    }
}
