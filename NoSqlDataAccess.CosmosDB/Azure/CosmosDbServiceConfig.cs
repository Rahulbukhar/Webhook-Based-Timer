

using Microsoft.Extensions.DependencyInjection;
using NoSqlDataAccess.Common;
using NoSqlDataAccess.Azure.CosmosDB.DBService;
using NoSqlDataAccess.Common.Provisioning;
using NoSqlDataAccess.Azure.CosmosDB.DBService.Internal;

namespace NoSqlDataAccess.Azure
{
    /// <summary>
    /// Provides extension methods to configure DB service.
    /// </summary>
    public static class CosmosDbServiceConfig
    {
        /// <summary>
        /// Extension method to configure CosmosDB service dependencies.
        /// </summary>
        /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
        /// <returns>Ann instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection ConfigureCosmosDb<TDBConnection, TDBProvisioningConfig>(this IServiceCollection services)
            where TDBConnection : class
            where TDBProvisioningConfig : class
        {
            services.AddSingleton<IAzureCosmosDbClientFactory, AzureCosmosDbClientFactory>();
            services.AddSingleton(typeof(ICosmosDbProvisioningConfiguration), typeof(TDBProvisioningConfig));
            services.AddSingleton(typeof(ICosmosDbConnection), typeof(TDBConnection));
            services.AddSingleton<ICosmosDbServiceFactory, CosmosDbServiceFactory>();

            services.AddSingleton(provider =>
            {
                var factory = provider.GetRequiredService<ICosmosDbServiceFactory>();
                return factory.CreateCosmosDbAdminService();
            });

            services.AddSingleton(provider =>
            {
                var factory = provider.GetRequiredService<ICosmosDbServiceFactory>();
                return factory.CreateCosmosDbService();
            });

            services.AddSingleton<INoSqlDbService, CosmosDbService>();
            services.AddSingleton<INoSqlDbAdminService, CosmosDbAdminService>();

            return services;
        }
    }
}
