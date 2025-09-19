using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace HealthMonitoringS.Models
{
    public class AlertEmailsRepository
    {
        private readonly CloudTable table;

        public AlertEmailsRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("AlertEmails");
            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task AddAlertEmailAsync(string email, string name = "")
        {
            AlertEmail alertEmail = new AlertEmail(email)
            {
                Name = name
            };

            TableOperation insertOperation = TableOperation.InsertOrReplace(alertEmail);
            await table.ExecuteAsync(insertOperation);
        }
    }
}

