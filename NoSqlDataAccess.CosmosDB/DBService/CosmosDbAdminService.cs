

using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

using System.Collections.Generic;


namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    using Common.Provisioning;
    using Common.Provisioning.Model;
    using NoSqlDataAccess.Common.Constants;

    /// <summary>
    /// Definition of Azure CosmosDB operations for administrators.
    /// </summary>
    public class CosmosDbAdminService : AbstractCosmosDbService, ICosmosDbAdminService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cosmosDbConnection">An instance of <see cref="ICosmosDbConnection"/>.</param>
        /// <param name="azureCosmosDbClientFactory">An instance of <see cref="IAzureCosmosDbClientFactory"/>.</param>
        /// <param name="cosmosDbProvisioning">An instance of <see cref="ICosmosDbProvisioningConfiguration"/>.</param>
        public CosmosDbAdminService(ICosmosDbConnection cosmosDbConnection, IAzureCosmosDbClientFactory azureCosmosDbClientFactory, ICosmosDbProvisioningConfiguration cosmosDbProvisioning) :
            base(cosmosDbConnection, azureCosmosDbClientFactory, cosmosDbProvisioning)
        { }

        /// <summary>
        /// Creates a CosmosDB table with given options.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of the create table operation.</returns>
        public async Task<string> CreateTableAsync(string tableName)
        {
            if (!tableConfigMap.TryGetValue(tableName, out ContainerConfiguration tableConfiguration))
            {
                return TableConstants.TABLE_CONFIG_NOT_FOUND;
            }

            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);

                int? ttlValue = TtlHelper(tableConfiguration);

                var tableProperties = new ContainerProperties(tableName, tableConfiguration.Schema.CosmosDBKeyConfiguration.PartitionKeyPath)
                {
                    DefaultTimeToLive = ttlValue
                };

                await database.CreateContainerAsync(tableProperties);

                return TableConstants.TABLE_CREATED;
            }
            catch (Exception ex)
            {
                if (((CosmosException)ex).StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Console.WriteLine($"CosmosDB table with name '{tableName}'. already exist.");
                    return TableConstants.TABLE_ALREADY_EXIST;
                }

                Console.WriteLine($"Failed to create CosmosDB table '{tableName}'. Reason: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Returns table information using database and table names.
        /// </summary>
        /// <param name="tableNames">List of table names.</param>
        /// <returns>A <see cref="Task"/> which on completion would describe tables by given table names.</returns>
        public async Task<List<JObject>> DescribeTablesAsync(List<string> tableNames)
        {
            var result = new List<JObject>();
            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);

                foreach (var tableName in tableNames)
                {
                    var table = database.GetContainer(tableName);
                    var tableResponse = await table.ReadContainerAsync();
                    var tableProperties = tableResponse.Resource;

                    var tableInfo = new JObject
                    {
                        ["id"] = tableProperties.Id,
                        ["partitionKeyPath"] = tableProperties.PartitionKeyPath,
                        ["indexingPolicy"] = JToken.FromObject(tableProperties.IndexingPolicy),
                        ["uniqueKeyPolicy"] = JToken.FromObject(tableProperties.UniqueKeyPolicy),
                        ["throughput"] = tableResponse.RequestCharge
                    };

                    result.Add(tableInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to describe CosmosDB tables. Reason: {ex.Message}", ex);
            }
            return result;
        }

        /// <summary>
        /// Updates the Time-To-Live setting of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of Time-To-Live setting update.</returns>
        public async Task<string> UpdateTimeToLiveAsync(string tableName)
        {
            string status = null;

            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);

                tableConfigMap.TryGetValue(tableName, out ContainerConfiguration tableConfiguration);
                if (tableConfiguration != null)
                {
                    var ttlConfig = tableConfiguration.CosmosDBTimeToLiveConfig;
                    if (ttlConfig != null)
                    {

                        var table = database.GetContainer(tableName);

                        var tableProperties = await table.ReadContainerAsync();
                        tableProperties.Resource.DefaultTimeToLive = ttlConfig.TtlInSeconds;
                        await table.ReplaceContainerAsync(tableProperties.Resource);
                        status = TableConstants.TTL_UPDATED;
                        Console.WriteLine($"Updated Time-To-Live setting for CosmosDB table: '{tableName}'. ");

                    }
                    else
                    {
                        status = TableConstants.TTL_NOT_CONFIGURED;
                        Console.WriteLine($"Time-To-Live setting is not configured for table '{tableConfiguration.ContainerName}'");
                    }
                }
                else
                {
                    status = TableConstants.TABLE_NOT_FOUND;
                }
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update ttl for CosmosDB table '{tableName}'. Reason: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a CosmosDB table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of the delete table operation.</returns>
        public async Task<string> DeleteContainerAsync(string tableName)
        {
            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);
                var table = database.GetContainer(tableName);

                await table.DeleteContainerAsync();
                return TableConstants.TABLE_DELETED;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return TableConstants.TABLE_NOT_FOUND;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete the CosmosDB table '{tableName}'. Reason: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Lists all tables in a given CosmosDB database.
        /// </summary>
        /// <returns>A <see cref="Task"/> which on completion would return a list of table names.</returns>
        public async Task<List<string>> ListContainersAsync()
        {
            var tableNames = new List<string>();
            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);
                var tables = database.GetContainerQueryIterator<ContainerProperties>();

                while (tables.HasMoreResults)
                {
                    var response = await tables.ReadNextAsync();
                    foreach (var table in response)
                    {
                        tableNames.Add(table.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to list the CosmosDB tables. Reason: {ex.Message}", ex);
            }
            return tableNames;
        }

        /// <summary>
        /// Retrieves the throughput settings of a CosmosDB table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the throughput settings of the table.</returns>
        public async Task<int> GetContainerThroughputAsync(string tableName)
        {
            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);
                var table = database.GetContainer(tableName);
                var throughputResponse = await table.ReadThroughputAsync();
                return throughputResponse.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get the CosmosDB table '{tableName}' throughput. Reason: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Updates the throughput settings of a CosmosDB table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="throughput">The new throughput value to be set.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of the throughput update operation.</returns>
        public async Task<string> UpdateContainerThroughputAsync(string tableName, int throughput)
        {
            try
            {
                var database = CosmosDbClient.GetDatabase(DatabaseName);
                var table = database.GetContainer(tableName);

                await table.ReplaceThroughputAsync(throughput);
                return TableConstants.THROUGHPUT_UPDATED;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get the CosmosDB table '{tableName}' throughput. Reason: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Adds or removes global secondary indexes on the given table.
        /// The global secondary index definitions provided on the <see cref="IDynamoDbProvisioningConfiguration"/> are considered for addition or removal of indexes.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would perform addition or removal of global secondary indexes on given table.</returns>
        public Task<string> AddOrRemoveGlobalSecondaryIndexesAsync(string tableName)
        {
            throw new NotImplementedException();
        }

        private static int? TtlHelper(ContainerConfiguration tableConfiguration)
        {
            if (tableConfiguration?.CosmosDBTimeToLiveConfig != null && tableConfiguration.CosmosDBTimeToLiveConfig.Enabled)
            {
                var defaultValue = tableConfiguration.CosmosDBTimeToLiveConfig.TtlInSeconds;
                if (defaultValue > 0)
                {
                    return defaultValue;
                }
                return -1;
            }
            return null;
        }

    }
}
