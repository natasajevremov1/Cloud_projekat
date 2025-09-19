/*ing Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.

namespace NotificationS.Entities
{
        public class NotificationsRepository
        {
            private readonly CloudTable _table;
            private readonly CloudTableClient _tableClient;
            public NotificationsRepository()
            {
                var storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("DataConnectionString"));

                _tableClient = storageAccount.CreateCloudTableClient();
                _table = _tableClient.GetTableReference("Notifications");
                _table.CreateIfNotExistsAsync().Wait();
            }

            public async Task SaveNotificationAsync(string topAnswerId, int emailCount)
            {
                var entity = new NotificationTableEntity(topAnswerId, emailCount);
                var insert = TableOperation.Insert(entity);
                await _table.ExecuteAsync(insert);
            }
        }
    
}*/
