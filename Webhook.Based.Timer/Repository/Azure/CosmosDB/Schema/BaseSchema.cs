

using NoSqlDataAccess.Common.Provisioning.Model;

namespace DatasetService.Core.Repository.Azure.CosmosDB.Schema
{
    /// <summary>
    /// Base schema class for Cosmos DB.
    /// </summary>
    internal static class BaseSchema
    {
        /// <summary>
        /// Adds common container properties to the container schema.
        /// </summary>
        /// <param name="containerSchema">Container schema.</param>
        /// <returns>Updated container schema with common properties.</returns>
        internal static ContainerSchema AddCommonContainerAttributes(ContainerSchema containerSchema)
        {
            if (containerSchema.Attributes == null)
            {
                containerSchema.Attributes = new List<ContainerAttribute>();
            }

            containerSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "createdBy",
                Type = ContainerAttributeType.String
            });
            containerSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "lastModifiedBy",
                Type = ContainerAttributeType.String
            });
            containerSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "creationDate",
                Type = ContainerAttributeType.DateTime
            });
            containerSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "lastModifiedDate",
                Type = ContainerAttributeType.DateTime
            });

            return containerSchema;
        }

        /// <summary>
        /// Creates an index name for Cosmos DB.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="partitionKey">Name of the partition key attribute.</param>
        /// <param name="sortKey">Name of the sort key attribute (if applicable).</param>
        /// <returns>Index name string.</returns>
        internal static string GetIndexName(string serviceName, string containerName, string partitionKey, string sortKey)
        {
            if (!string.IsNullOrWhiteSpace(sortKey))
            {
                return $"{serviceName}_{containerName}_{partitionKey}_{sortKey}_idx";
            }

            return $"{serviceName}_{containerName}_{partitionKey}_idx";
        }
    }
}
