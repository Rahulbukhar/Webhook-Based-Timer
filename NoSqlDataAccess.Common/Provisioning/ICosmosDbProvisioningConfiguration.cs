

using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Provisioning
{
    using Model;

    /// <summary>
    /// Definition of CosmosDB provisioning configuration.
    /// </summary>
    public interface ICosmosDbProvisioningConfiguration
    {
        /// <summary>
        /// List of table configurations.
        /// </summary>
        List<ContainerConfiguration> ContainerConfigurations { get; }
    }
}
