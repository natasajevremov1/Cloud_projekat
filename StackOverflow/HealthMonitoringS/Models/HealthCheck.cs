using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Concurrent;

namespace HealthMonitoringS.Models
{
    public class HealthCheck : TableEntity
    {
        public HealthCheck() { }

        public HealthCheck(string serviceName)
        {
            PartitionKey = serviceName;        // naziv servisa
            RowKey = Guid.NewGuid().ToString(); // jedinstveni ID
        }

        public string Status { get; set; } // "OK" ili "NOT_OK"


        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
