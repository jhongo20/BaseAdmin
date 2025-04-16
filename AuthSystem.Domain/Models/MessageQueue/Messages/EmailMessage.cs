using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models.MessageQueue.Messages
{
    public class EmailMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = true;
        public List<EmailAttachmentInfo> Attachments { get; set; } = new List<EmailAttachmentInfo>();
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public int Priority { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public bool SendImmediately { get; set; } = false;
        public DateTime? ScheduledFor { get; set; }
    }

    public class EmailAttachmentInfo
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
