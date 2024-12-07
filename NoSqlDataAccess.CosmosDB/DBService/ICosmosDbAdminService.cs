

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    using Common;

    /// <summary>
    /// Definition of Azure CosmosDB operations for administrators.
    /// </summary>
    public interface ICosmosDbAdminService : INoSqlDbAdminService
    {
        /// <summary>
        /// Deletes a CosmosDB table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of the delete table operation.</returns>
        Task<string> DeleteContainerAsync(string tableName);

        /// <summary>
        /// Lists all tables in a given CosmosDB database.
        /// </summary>
        /// <returns>A <see cref="Task"/> which on completion would return a list of table names.</returns>
        Task<List<string>> ListContainersAsync();

        /// <summary>
        /// Retrieves the throughput settings of a CosmosDB table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the throughput settings of the table.</returns>
        Task<int> GetContainerThroughputAsync(string tableName);

        /// <summary>
        /// Updates the throughput settings of a CosmosDB table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="throughput">The new throughput value to be set.</param>
        /// <returns>A <see cref="Task"/> which on completion would return the status of the throughput update operation.</returns>
        Task<string> UpdateContainerThroughputAsync(string tableName, int throughput);
    }
}
