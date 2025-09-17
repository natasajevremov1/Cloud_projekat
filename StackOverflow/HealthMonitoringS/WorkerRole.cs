using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue; // dodaj za queue
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoringS.Models;
using System.Net;

namespace HealthMonitoringS
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private HttpClient httpClient;
        private CloudQueue alertsQueue; // queue za NOT_OK dogaðaje

        public override bool OnStart()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 12;

            httpClient = new HttpClient();

            // Inicijalizacija alerts queue
            var connectionString = RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            alertsQueue = queueClient.GetQueueReference("alerts");
            alertsQueue.CreateIfNotExists();

            bool result = base.OnStart();
            Trace.TraceInformation("HealthMonitoringS has been started");
            return result;
        }

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringS is running");
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringS is stopping");
            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();
            base.OnStop();
            Trace.TraceInformation("HealthMonitoringS has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // Azure Table
            string connectionString = RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable healthTable = tableClient.GetTableReference("HealthCheck");
            healthTable.CreateIfNotExists();

            while (!cancellationToken.IsCancellationRequested)
            {
                await CheckServiceAsync("StackOverflowService", "http://localhost:5059/health-monitoring", healthTable);
                await CheckServiceAsync("NotificationService", "http://localhost:5060/health-monitoring", healthTable);

                await Task.Delay(4000); // 4 sekunde
            }
        }

        private async Task CheckServiceAsync(string serviceName, string url, CloudTable table)
        {
            // kreiramo entitet i prosleðujemo serviceName u konstruktor
            HealthCheck entity = new HealthCheck(serviceName)
            {
                CheckedAt = DateTime.UtcNow
            };

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                entity.Status = response.IsSuccessStatusCode ? "OK" : "NOT_OK";
            }
            catch
            {
                entity.Status = "NOT_OK";
            }

            // Upis u tabelu
            TableOperation insertOperation = TableOperation.Insert(entity);
            await table.ExecuteAsync(insertOperation);

            // Ako je NOT_OK, šaljemo poruku u alerts queue
            if (entity.Status == "NOT_OK")
            {
                string alertMessage = $"{serviceName} nije dostupan u {DateTime.UtcNow}";
                CloudQueueMessage message = new CloudQueueMessage(alertMessage);
                await alertsQueue.AddMessageAsync(message);
            }
        }
    }
}
