

using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Provisioning
{
    using Common.Provisioning.Model;

    /// <summary>
    /// Base class for CosmosDB provisioning configuration.
    /// </summary>
    public class BaseCosmosDbProvisioningConfiguration : ICosmosDbProvisioningConfiguration
    {
        /// <summary>
        /// List of table configurations.
        /// </summary>
        public List<ContainerConfiguration> ContainerConfigurations => tableConfigs;

        /// <summary>
        /// Container configurations value.
        /// </summary>
        private readonly List<ContainerConfiguration> tableConfigs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tableConfigurations">List of table connfigurations.</param>
        public BaseCosmosDbProvisioningConfiguration(List<ContainerConfiguration> tableConfigurations)
        {
            tableConfigs = new List<ContainerConfiguration>(tableConfigurations);
        }
    }
}
