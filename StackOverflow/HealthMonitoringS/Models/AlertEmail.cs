using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitoringS.Models
{
        public class AlertEmail : TableEntity
        {
            public AlertEmail() { }

            public AlertEmail(string email)
            {
                PartitionKey = "Admin";   // može biti fiksni PartitionKey
                RowKey = email;           // mejl kao jedinstveni RowKey
                Email = email;
                AddedAt = DateTime.UtcNow;
            }

            public string Email { get; set; }
            public DateTime AddedAt { get; set; }
            public string Name { get; set; }  // opciono ime administratora
        }
}
