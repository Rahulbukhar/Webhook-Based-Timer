


namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    using Common.Provisioning;

    /// <summary>
    /// Definition of a CosmosDB Connection.
    /// </summary>
    public interface ICosmosDbConnection
    {
        /// <summary>
        /// The endpoint URI for accessing the CosmosDB service.
        /// </summary>
        string EndpointUri { get; }

        /// <summary>
        /// The primary key used for authenticating access to CosmosDB.
        /// </summary>
        string PrimaryKey { get; }

        /// <summary>
        /// The name of the database to be accessed.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Provisioning configuration for CosmosDB.
        /// </summary>
        ICosmosDbProvisioningConfiguration ProvisioningConfig { get; }
    }
}
