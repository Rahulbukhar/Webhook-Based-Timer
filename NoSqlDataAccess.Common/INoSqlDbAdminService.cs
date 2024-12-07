

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoSqlDataAccess.Common
{
    using Provisioning;

    /// <summary>
    /// Common interface for NoSQL database admin operations.
    /// </summary>
    public interface INoSqlDbAdminService
    {
        /// <summary>
        /// Creates a CosmosDB table with given options.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return status of the create table operation..</returns>
        Task<string> CreateTableAsync(string tableName);

        /// <summary>
        /// Returns table information using table names.
        /// </summary>
        /// <param name="tableNames">List of table names.</param>
        /// <returns>A <see cref="Task"/> which on completion would describe tables by given table names.</returns>
        Task<List<JObject>> DescribeTablesAsync(List<string> tableNames);

        /// <summary>
        /// Updates the Time-To-Live setting of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of Time-To-Live setting update.</returns>
        Task<string> UpdateTimeToLiveAsync(string tableName);

        /// <summary>
        /// Adds or removes global secondary indexes on the given table.
        /// The global secondary index definitions provided on the <see cref="IDynamoDbProvisioningConfiguration"/> are considered for addition or removal of indexes.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would perform addition or removal of global secondary indexes on given table.</returns>
        Task<string> AddOrRemoveGlobalSecondaryIndexesAsync(string tableName);
    }
}
