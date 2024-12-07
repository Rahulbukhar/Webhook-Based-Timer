

namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    /// <summary>
    /// Definition of a factory for CosmosDB Service.
    /// </summary>
    public interface ICosmosDbServiceFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ICosmosDbService"/>.
        /// </summary>
        /// <returns>An instance of <see cref="ICosmosDbService"/>.</returns>
        ICosmosDbService CreateCosmosDbService();

        /// <summary>
        /// Creates an instance of <see cref="ICosmosDbAdminService"/>.
        /// </summary>
        /// <returns>An instance of <see cref="ICosmosDbAdminService"/>.</returns>
        ICosmosDbAdminService CreateCosmosDbAdminService();
    }
}
