using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models.MessageQueue.Messages
{
    public class ReportMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ReportType { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string OutputFormat { get; set; } = "PDF";
        public string RequestedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool NotifyOnCompletion { get; set; } = true;
        public string NotificationEmail { get; set; }
        public string CallbackUrl { get; set; }
        public int Priority { get; set; } = 1;
        public DateTime? ScheduledFor { get; set; }
        public string OutputPath { get; set; }
    }
}
