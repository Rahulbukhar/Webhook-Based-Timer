

using System.Collections.Generic;

namespace NoSqlDataAccess.Azure.CosmosDB.Schema
{
    using Common.Provisioning.Model;
    using Test.Data.Schema;

    /// <summary>
    /// This class provides data model for Dataset.
    /// </summary>
    internal static class VaultSchema
    {
        /// <summary>
        /// Schema
        /// </summary>
        public static readonly ContainerSchema Schema = CreateContainerSchema();

        private static ContainerSchema CreateContainerSchema()
        {
            ContainerSchema schema = new()
            {
                Attributes = new List<ContainerAttribute>
                {
                    new() { Name = "vaultId", Type = ContainerAttributeType.String },
                    new() { Name = "accountId", Type = ContainerAttributeType.String },
                    new() { Name = "kmsKeyArn", Type = ContainerAttributeType.String },
                    new() { Name = "backupKmsKeyArn", Type = ContainerAttributeType.String },
                    new() { Name = "bucketAlias", Type = ContainerAttributeType.String },
                    new() { Name = "status", Type = ContainerAttributeType.String },
                    new() { Name = "s3FolderName", Type = ContainerAttributeType.String },
                    new() { Name = "scanOptions", Type = ContainerAttributeType.Object }
                },
                CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration
                {
                    PartitionKeyPath = $"/vaultId"
                }
            };

            return BaseSchema.AddCommonContainerAttributes(schema);
        }

    }
}
