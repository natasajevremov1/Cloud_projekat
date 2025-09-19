using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationS.Entities
{
    public class NotificationTableEntity : TableEntity
    {
        public NotificationTableEntity() { }

        public NotificationTableEntity(string topAnswerId, int emailCount)
        {
            PartitionKey = "Notification";
            RowKey = Guid.NewGuid().ToString();
            TopAnswerId = topAnswerId;
            EmailCount = emailCount;
            SentAt = DateTime.UtcNow;
        }

        public string TopAnswerId { get; set; }
        public int EmailCount { get; set; }
        public DateTime SentAt { get; set; }
    }
}
