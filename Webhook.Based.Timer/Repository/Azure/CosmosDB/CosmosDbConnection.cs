

using NoSqlDataAccess.Common.Constants;
using NoSqlDataAccess.Common.Provisioning;
using NoSqlDataAccess.Azure.CosmosDB.DBService;
using WebhookBasedTimer.Model;

namespace DatasetService.Core.Repository.Azure.CosmosDB
{
    /// <summary>
    /// Implementation of <see cref="ICosmosDbConnection"/>
    /// </summary>
    internal class CosmosDbConnection : ICosmosDbConnection
    {
        #region Public Members

        /// <summary>
        /// The endpoint URI for accessing the CosmosDB service.
        /// </summary>
        public string EndpointUri { get { return endpointUri; } }

        /// <summary>
        /// The primary key used for authenticating access to CosmosDB.
        /// </summary>
        public string PrimaryKey { get { return primaryKey; } }

        /// <summary>
        /// The name of the database to be accessed.
        /// </summary>
        public string DatabaseName { get { return databaseName; } }

        /// <summary>
        /// Provisioning configuration for CosmosDB.
        /// </summary>
        public ICosmosDbProvisioningConfiguration ProvisioningConfig { get { return provisioningConfig; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configProvider">An instance of <see cref="IConfigProvider"/>.</param>
        /// <param name="provisioningConfig">An instance of <see cref="ICosmosDbProvisioningConfiguration"/>.</param>
        public CosmosDbConnection(IConfigProvider configProvider, ICosmosDbProvisioningConfiguration provisioningConfig)
        {
            endpointUri = configProvider.GetConfig(CosmosDbConfigurationConstants.COSMOSDB_ACCOUNT_ENDPOINT);
            primaryKey = configProvider.GetConfig(CosmosDbConfigurationConstants.COSMOSDB_ACCOUNT_KEY);
            databaseName = configProvider.GetConfig(CosmosDbConfigurationConstants.COSMOSDB_DATABASE_NAME);
            this.provisioningConfig = provisioningConfig;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// The endpoint URI for accessing the CosmosDB service.
        /// </summary>
        private string endpointUri { get; }

        /// <summary>
        /// The primary key used for authenticating access to CosmosDB.
        /// </summary>
        private string primaryKey { get; }

        /// <summary>
        /// The name of the database to be accessed.
        /// </summary>
        private string databaseName { get; }

        /// <summary>
        /// Provisioning configuration for CosmosDB.
        /// </summary>
        private ICosmosDbProvisioningConfiguration provisioningConfig { get; }

        #endregion
    }
}
