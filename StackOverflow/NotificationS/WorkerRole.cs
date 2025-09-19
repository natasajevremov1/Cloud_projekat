using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using NotificationS.Helper;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private CloudQueue queue;
        private CloudTable table;

        public override void Run()
        {
            Trace.TraceInformation("NotificationService is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 12;

            // Kreiranje queue i tabele
            queue = QueueHelper.GetQueueReference("emails");
            Trace.TraceInformation("Queue 'emails' ensured to exist.");

            table = TableHelper.GetTable("NotificationTable");
            Trace.TraceInformation("Table 'NotificationTable' ensured to exist.");

            // Ubacivanje jednog reda u tabelu
            var entity = new DynamicTableEntity("Partition1", Guid.NewGuid().ToString());
            entity.Properties.Add("Message", new EntityProperty("Hello World!"));
            var insertOperation = TableOperation.InsertOrMerge(entity);
            table.Execute(insertOperation);
            Trace.TraceInformation("Inserted a row into 'NotificationTable'.");

            // Ubacivanje poruke u queue
            queue.AddMessage(new CloudQueueMessage("Hello queue!"));
            Trace.TraceInformation("Added message to 'emails' queue.");

            bool result = base.OnStart();
            Trace.TraceInformation("NotificationService has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NotificationService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("NotificationService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
