

using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Provisioning.Model
{
    #region CosmosDB

    /// <summary>
    /// Data structure to represent Cosmos DB Container configuration.
    /// </summary>
    public class ContainerConfiguration
    {
        /// <summary>
        /// Throughput settings for the table.
        /// </summary>
        public int? Throughput { get; set; }

        /// <summary>
        /// Flag to indicate whether delete protection is enabled for the table.
        /// </summary>
        public bool DeleteProtectionEnabled { get; set; }

        /// <summary>
        /// Container schema including partition key and optional sort key.
        /// </summary>
        public ContainerSchema Schema { get; set; }

        /// <summary>
        /// Flat to indicate wheter to handle the `Id` field processing seperately.
        /// </summary>
        public bool HandleIdFieldProcessing { get; set; } = false;

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Time-To-Live configuration for the table.
        /// </summary>
        public CosmosDBTimeToLiveConfig CosmosDBTimeToLiveConfig { get; set; }

        /// <summary>
        /// Tags to store additional categorization information.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
    }

    /// <summary>
    /// Data structure to represent the schema of a Cosmos DB table.
    /// </summary>
    public class ContainerSchema
    {
        /// <summary>
        /// Configuration of primary (partition) and optional sort key fields for the table.
        /// </summary>
        public CosmosDBKeyConfiguration CosmosDBKeyConfiguration { get; set; }

        /// <summary>
        /// List of attributes in the table.
        /// </summary>
        public List<ContainerAttribute> Attributes { get; set; }

        /// <summary>
        /// Adds common table attributes, such as system metadata.
        /// </summary>
        /// <param name="schema">The table schema to which common attributes will be added.</param>
        /// <returns>The updated table schema.</returns>
        public static ContainerSchema AddCommonContainerAttributes(ContainerSchema schema)
        {
            if (schema.Attributes == null)
            {
                schema.Attributes = new List<ContainerAttribute>();
            }

            // Example: Adding a common attribute (e.g., createdAt)
            schema.Attributes.Add(new ContainerAttribute
            {
                Name = "createdAt",
                Type = ContainerAttributeType.DateTime
            });

            return schema;
        }
    }

    /// <summary>
    /// Data structure to represent key configuration in Cosmos DB.
    /// </summary>
    public class CosmosDBKeyConfiguration
    {
        /// <summary>
        /// The partition key path for the table.
        /// </summary>
        public string PartitionKeyPath { get; set; }

        /// <summary>
        /// The sort key path for the table.
        /// Optional: Cosmos DB does not have a direct equivalent of a sort key.
        /// </summary>
        public string SortKeyPath { get; set; }
    }

    /// <summary>
    /// Data structure to represent attributes for a Cosmos DB table.
    /// </summary>
    public class ContainerAttribute
    {
        /// <summary>
        /// Name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DB attibute Mapping Name.
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// Data type of the attribute.
        /// </summary>
        public ContainerAttributeType Type { get; set; }
    }

    /// <summary>
    /// Enumeration that represents the possible data types of attributes within a Cosmos DB table.
    /// These data types define the nature of the data stored and how it is processed within the database.
    /// </summary>
    public enum ContainerAttributeType
    {
        /// <summary>
        /// Represents a string data type, used for textual data.
        /// </summary>
        String,

        /// <summary>
        /// Represents a numeric data type, typically used for floating-point numbers.
        /// </summary>
        Number,

        /// <summary>
        /// Represents a long integer data type, used for storing large integer values.
        /// </summary>
        Long,

        /// <summary>
        /// Represents a DateTime data type, used for storing date and time values.
        /// </summary>
        DateTime,

        /// <summary>
        /// Represents a Boolean data type, used for storing true/false values.
        /// </summary>
        Boolean,

        /// <summary>
        /// Represents an object data type, used for storing complex objects or JSON structures.
        /// </summary>
        Object
    }

    /// <summary>
    /// Data structure to represent Time-To-Live configuration.
    /// </summary>
    public class CosmosDBTimeToLiveConfig
    {
        /// <summary>
        /// Name of the attribute used for TTL.
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Flag to indicate whether Time-To-Live is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Time-To-Live value.
        /// </summary>
        public int TtlInSeconds { get; set; }
    }
    #endregion CosmosDB
}
