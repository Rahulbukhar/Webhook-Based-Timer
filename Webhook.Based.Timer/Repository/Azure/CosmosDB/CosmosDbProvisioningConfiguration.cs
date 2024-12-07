

using DatasetService.Core.Repository.Azure.CosmosDB.Schema;
using NoSqlDataAccess.Common.Provisioning;
using NoSqlDataAccess.Common.Provisioning.Model;

namespace DatasetService.Core.Repository.Azure.CosmosDB
{
    /// <summary>
    /// Configuration to provision CosmosDB.
    /// </summary>
    internal class CosmosDbProvisioningConfiguration : BaseCosmosDbProvisioningConfiguration
    {
        /// <summary>
        /// Constant for purpose
        /// </summary>
        private const string purpose = "purpose";

        /// <summary>
        /// Constructor
        /// </summary>
        public CosmosDbProvisioningConfiguration() : base(GetContainerConfigurations()) { }

        /// <summary>
        /// Returns table configurations.
        /// </summary>
        private static List<ContainerConfiguration> GetContainerConfigurations()
        {
            return new()
            {
                new ContainerConfiguration
                {
                    Throughput = null,  // Pay-per-request is default in Cosmos DB when Throughput is null.
                    DeleteProtectionEnabled = true,
                    Schema = TimerSchema.Schema,
                    ContainerName = ContainerNames.TIMER_CONTAINER_NAME,
                    Tags = new Dictionary<string, string>
                    {
                        { purpose, "stores timers" }
                    },
                    CosmosDBTimeToLiveConfig = null // TTL not configured
                }
            };
        }


    }
}
