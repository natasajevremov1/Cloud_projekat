using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    public class NotificationLog : TableEntity
    {
        public NotificationLog(string answerId)
        {
            PartitionKey = "Notification";
            RowKey = answerId;
        }

        public NotificationLog() { }

        public int EmailsSent { get; set; }
        public DateTime SentAt { get; set; }
    }

}