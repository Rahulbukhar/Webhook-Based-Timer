


namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    using Common.Provisioning;

    /// <summary>
    /// Implementation of <see cref="ICosmosDbServiceFactory"/>.
    /// </summary>
    public class CosmosDbServiceFactory : ICosmosDbServiceFactory
    {
        /// <summary>
        /// An instance of <see cref="IAzureCosmosDbClientFactory"/>.
        /// </summary>
        private readonly IAzureCosmosDbClientFactory cosmosDbClientFactory;

        /// <summary>
        /// An instance of <see cref="ICosmosDbProvisioningConfiguration"/>.
        /// </summary>
        private readonly ICosmosDbProvisioningConfiguration cosmosDbProvisioningConfig;

        /// <summary>
        /// An instance of <see cref="ICosmosDbConnection"/>.
        /// </summary>
        private readonly ICosmosDbConnection cosmosDbConnection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cosmosDbConnection">An instance of <see cref="ICosmosDbConnection"/>.</param>
        /// <param name="cosmosDbClientFactory">An instance of <see cref="IAzureCosmosDbClientFactory"/>.</param>
        /// <param name="cosmosDbProvisioningConfig">An instance of <see cref="ICosmosDbProvisioningConfiguration"/>.</param>
        public CosmosDbServiceFactory(ICosmosDbConnection cosmosDbConnection, IAzureCosmosDbClientFactory cosmosDbClientFactory, ICosmosDbProvisioningConfiguration cosmosDbProvisioningConfig)
        {
            this.cosmosDbConnection = cosmosDbConnection;
            this.cosmosDbClientFactory = cosmosDbClientFactory;
            this.cosmosDbProvisioningConfig = cosmosDbProvisioningConfig;
        }

        /// <summary>
        /// Creates an instance of <see cref="ICosmosDbService"/>.
        /// </summary>
        /// <returns>An instance of <see cref="ICosmosDbService"/>.</returns>
        public ICosmosDbService CreateCosmosDbService()
        {
            return new CosmosDbService(cosmosDbConnection, cosmosDbClientFactory, cosmosDbProvisioningConfig);
        }

        /// <summary>
        /// Creates an instance of <see cref="ICosmosDbAdminService"/>.
        /// </summary>
        /// <returns>An instance of <see cref="ICosmosDbAdminService"/>.</returns>
        public ICosmosDbAdminService CreateCosmosDbAdminService()
        {
            return new CosmosDbAdminService(cosmosDbConnection, cosmosDbClientFactory, cosmosDbProvisioningConfig);
        }
    }
}
