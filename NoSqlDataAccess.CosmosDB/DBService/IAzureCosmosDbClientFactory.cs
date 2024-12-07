

using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    /// <summary>
    /// Definition of the factory to create Amazon CosmosDB Client
    /// </summary>
    public interface IAzureCosmosDbClientFactory
    {
        /// <summary>
        /// Returns an instance of a CosmosDB client.
        /// </summary>
        /// <param name="cosmosEndPoint">The Cosmos DB account endpoint.</param>
        /// <param name="cosmosAuthOrResourceKey">The Cosmos DB authorization or resource token key.</param>
        /// <returns>An instance of <see cref="CosmosClient"/>.</returns>
        Task<CosmosClient> GetCosmosDbClient(string cosmosEndPoint, string cosmosAuthOrResourceKey);
    }
}
