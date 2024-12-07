

using System.Collections.Generic;
using NoSqlDataAccess.Common.Provisioning.Model;

namespace NoSqlDataAccess.Azure.CosmosDB.Test.Data.Schema
{
    /// <summary>
    /// Base schema class for Cosmos DB.
    /// </summary>
    internal static class BaseSchema
    {
        /// <summary>
        /// Adds common table properties to the table schema.
        /// </summary>
        /// <param name="tableSchema">Container schema.</param>
        /// <returns>Updated table schema with common properties.</returns>
        internal static ContainerSchema AddCommonContainerAttributes(ContainerSchema tableSchema)
        {
            if (tableSchema.Attributes == null)
            {
                tableSchema.Attributes = new List<ContainerAttribute>();
            }

            tableSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "createdBy",
                Type = ContainerAttributeType.String
            });
            tableSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "lastModifiedBy",
                Type = ContainerAttributeType.String
            });
            tableSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "creationDate",
                Type = ContainerAttributeType.DateTime
            });
            tableSchema.Attributes.Add(new ContainerAttribute
            {
                Name = "lastModifiedDate",
                Type = ContainerAttributeType.DateTime
            });

            return tableSchema;
        }
    }
}
