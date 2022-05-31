using ASC.DataAccess.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : TableEntity, new ()
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable storageTable;

        public IUnitOfWork Scope { get; set; }

        public Repository(IUnitOfWork scope)
        {
            storageAccount = CloudStorageAccount.Parse(scope.connectionString);

            tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(typeof(T).Name);

            this.storageTable = table;
            this.Scope = scope;

        }

        private async Task<Action> CreateRollBackAction(TableOperation operation)
        {
            if (operation.OperationType == TableOperationType.Retrieve) return null;

            var tableEntity = operation.Entity;
            var cloudTable = storageTable;

            switch (operation.OperationType)
            {
                case TableOperationType.Insert:
                    return async () => await UndoInsertOperationAsync(cloudTable, tableEntity);
                case TableOperationType.Delete:
                    return async() => await UndoDeleteOperation(cloudTable, tableEntity);
                case TableOperationType.Replace:
                    var retrieveResult = await cloudTable.ExecuteAsync(TableOperation
                        .Retrieve(tableEntity.PartitionKey, tableEntity.RowKey));
                    return async () => await UndoReplaceOperation(cloudTable, retrieveResult.Result as DynamicTableEntity, tableEntity);
                default:
                    throw new InvalidOperationException("The storage operation cannot be identified");

            }
        }

        private async Task UndoInsertOperationAsync(CloudTable table, ITableEntity entity)
        {
            var deleteOperation = TableOperation.Delete(entity);
            await table.ExecuteAsync(deleteOperation);
        }
        private async Task UndoDeleteOperation(CloudTable table, ITableEntity entity)
        {
            var entityToRestore = entity as BaseEntity;
            entityToRestore.IsDeleted = false;
            var insertOperation = TableOperation.Replace(entity);
            await table.ExecuteAsync(insertOperation);
        }
        private async Task UndoReplaceOperation(CloudTable table, ITableEntity originalEntity,
        ITableEntity newEntity)
        {
            if (originalEntity != null)
            {
                if (!String.IsNullOrEmpty(newEntity.ETag)) originalEntity.ETag = newEntity.ETag;
                var replaceOperation = TableOperation.Replace(originalEntity);
                await table.ExecuteAsync(replaceOperation);
            }
        }
        public async Task<T> AddAsync(T entity)
        {
            var insert = entity as BaseEntity;
            insert.CreateDate = DateTime.UtcNow;
            insert.UpdateDate = DateTime.UtcNow;

            TableOperation operation = TableOperation.Insert(entity);
            return await ExecuteAsync(operation) as T;


        }

        public async Task CreateTableAsync()
        {
            CloudTable table = tableClient.GetTableReference(typeof(T).Name);
            await table.CreateIfNotExistsAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            var delete = entity as BaseEntity;
            delete.IsDeleted = true;
            delete.UpdateDate = DateTime.UtcNow;

            TableOperation operation = TableOperation.Replace(entity);
            await ExecuteAsync(operation);
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
           TableQuery<T> query = new TableQuery<T>();
            TableContinuationToken token = null;
            var result = await storageTable.ExecuteQuerySegmentedAsync(query, token);
            return result.Results as IEnumerable<T>;
        }

        public async Task<IEnumerable<T>> FindAllByPartitionKeyAsync(string partitionKey)
        {
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken token = null;
            var result = await storageTable.ExecuteQuerySegmentedAsync(query, token);
            return result.Results as IEnumerable<T>;

        }

        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result =    await storageTable.ExecuteAsync(operation);
            return result.Result as T;

        }

        public async Task<T> UpdateAsync(T entity)
        {
            var update = entity as BaseEntity;
            update.UpdateDate = DateTime.UtcNow;

            TableOperation operation = TableOperation.Replace(entity);
            var result = await ExecuteAsync(operation);
            return result.Result as T;
        }

        private async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            var rollbackAction = CreateRollBackAction(operation);
            var result = await storageTable.ExecuteAsync(operation);
            Scope.RollBackActions.Enqueue(rollbackAction);
            return result;
        }
    }
}
