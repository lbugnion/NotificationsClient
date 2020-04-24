using Microsoft.Azure.Cosmos.Table;
using System;
using System.Threading.Tasks;

namespace Notifications
{
    public class Storage
    {
        private static CloudStorageAccount _account;

        private CloudStorageAccount Account
        {
            get => _account ?? (_account = CloudStorageAccount.Parse(
                Environment.GetEnvironmentVariable(
                    Notifications.AzureWebJobsStorageName)));
        }

        public async Task<CloudTable> EnsureTable(string tableName)
        {
            var tableClient = Account.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<NotificationEntity> InsertOrMergeNotification(
            NotificationEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            // Create the InsertOrReplace table operation
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation
            var table = await EnsureTable(FunctionConstants.NotificationTableName);

            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            var insertedNotification = result.Result as NotificationEntity;

            return insertedNotification;
        }
    }
}
