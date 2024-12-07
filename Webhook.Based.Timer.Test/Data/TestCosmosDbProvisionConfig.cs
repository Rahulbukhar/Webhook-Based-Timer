

using System.Collections.Generic;

namespace NoSqlDataAccess.Azure.CosmosDB.Test.Data
{
    using Common.Provisioning;
    using Common.Provisioning.Model;
    using Data.Schema;
    using NoSqlDataAccess.Azure.CosmosDB.Schema;

    /// <summary>
    /// Configuration to provision CosmosDB.
    /// </summary>
    public class TestCosmosDbProvisionConfig : BaseCosmosDbProvisioningConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TestCosmosDbProvisionConfig() : base(GetContainerConfigurations()) { }

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
                    Schema = VaultSchema.Schema,
                    ContainerName = ContainerNames.VAULT_TABLE_NAME_TEST,
                    Tags = new Dictionary<string, string>
                    {
                        { "purpose", "stores vaults" }
                    },
                    CosmosDBTimeToLiveConfig = null // TTL not configured
                }
            };
        }

    }
}
