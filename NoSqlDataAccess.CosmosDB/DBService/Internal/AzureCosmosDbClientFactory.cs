

using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace NoSqlDataAccess.Azure.CosmosDB.DBService.Internal
{
    /// <summary>
    /// Implementation of a factory for creating CosmosDB clients.
    /// </summary>
    public class AzureCosmosDbClientFactory : IAzureCosmosDbClientFactory
    {
        /// <summary>
        /// Returns an instance of a CosmosDB client.
        /// </summary>
        /// <param name="cosmosEndPoint">The Cosmos DB account endpoint.</param>
        /// <param name="cosmosAuthOrResourceKey">The Cosmos DB authorization or resource token key.</param>
        /// <returns>An instance of <see cref="CosmosClient"/>.</returns>
        public async Task<CosmosClient> GetCosmosDbClient(string cosmosEndPoint, string cosmosAuthOrResourceKey)
        {
            CosmosClient client = new CosmosClient(
                accountEndpoint: cosmosEndPoint,
                authKeyOrResourceToken: cosmosAuthOrResourceKey
            );

            return await Task.FromResult(client);
        }
    }
}
