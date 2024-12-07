

using System;
using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Provisioning.Model
{
    /// <summary>
    /// Data structure to represent Table configuration.
    /// </summary>
    public class TableConfiguration
    {
        /// <summary>
        /// Capacity mode.
        /// </summary>
        public CapacityMode CapacityMode { get; set; }

        /// <summary>
        /// Flag to indicate whether delete protection is enabled for the table.
        /// </summary>
        public bool DeleteProtectionEnabled { get; set; }

        /// <summary>
        /// Encryption configuration.
        /// </summary>
        public EncryptionConfiguration EncryptionConfiguration { get; set; }

        /// <summary>
        /// Provisioned throughput for table.
        /// </summary>
        public ProvisionedThroughput ProvisionedThroughput { get; set; }

        /// <summary>
        /// Table schema.
        /// </summary>
        public TableSchema Schema { get; set; }

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Table class.
        /// </summary>
        public TableClass TableClass { get; set; }

        /// <summary>
        /// Tags to store additional categorization information.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Time-To-Live configuration.
        /// The value of the attribute must represent a date in UNIX Epoch format.
        /// </summary>
        public TimeToLiveConfig TimeToLiveConfig { get; set; }
    }

    /// <summary>
    /// Data structure to represent Schema of a DynamDB table.
    /// </summary>
    public class TableSchema
    {
        /// <summary>
        /// Configuration of primary/sort key fields for the table.
        /// </summary>
        public KeyConfiguration KeyConfiguration { get; set; }

        /// <summary>
        /// Attribute definitions.
        /// </summary>
        public List<TableAttribute> Attributes { get; set; }

        /// <summary>
        /// Name of the attribute to be used as delete marker.
        /// The value of this attribute must be boolean.
        /// </summary>
        public string DeleteMarkerAttributeName { get; set; }

        /// <summary>
        /// List of Global Secondary Index (GSI) definitions.
        /// </summary>
        public List<SecondaryIndex> GlobalSecondaryIndexes { get; set; }

        /// <summary>
        /// List of Local Secondary Index (LSI) definitions.
        /// </summary>
        public List<SecondaryIndex> LocalSecondaryIndexes { get; set; }
    }

    /// <summary>
    /// Data structure to represent an attribute.
    /// </summary>
    public class TableAttribute
    {
        /// <summary>
        /// Name of the attribute.
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Data type.
        /// </summary>
        public Type DataType { get; set; }
    }

    /// <summary>
    /// Data structure to represent a secondary index.
    /// </summary>
    public class SecondaryIndex
    {
        /// <summary>
        /// Name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Index projection.
        /// </summary>
        public IndexProjection Projection { get; set; }

        /// <summary>
        /// Configuration of primary/sort key attributes for the index.
        /// </summary>
        public KeyConfiguration KeyConfiguration { get; set; }

        /// <summary>
        /// Provisioned throughput for index.
        /// </summary>
        public ProvisionedThroughput ProvisionedThroughput { get; set; }
    }

    /// <summary>
    /// Data structure to represent Server Side Encryption specification.
    /// </summary>
    public class EncryptionConfiguration
    {
        /// <summary>
        /// Flag to indicatr
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The AWS KMS key to be used for KMS encryption.
        /// </summary>
        public string KmsMasterKeyId { get; set; }

        /// <summary>
        /// Encryption type.
        /// </summary>
        public EncryptionType EncryptionType { get; set; }
    }

    /// <summary>
    /// Data structure to represent key configuration.
    /// </summary>
    public class KeyConfiguration
    {
        /// <summary>
        /// The primary key attribute for the table.
        /// Also called 'Partition key' attribute.
        /// Also called 'HASH key' attribute.
        /// DynamoDB internally uses the value of this attribute in its internal hash function to distribute item data across physical disk partitions.
        /// The value of this key must be unique for all items in the table. An exception to uniqueness rule applies only when the composite primary key (primary key + sort key) approach is used.
        /// </summary>
        public string PrimaryKeyAttribute { get; set; }

        /// <summary>
        /// The sort key attribute for the table.
        /// Also called 'RANGE key' attribute.
        /// When specified, DynamoDB uses the value of this attribute, in addition to primary key attribute in its internal hash function to store items in the same partition close together, by the sory key value.
        /// </summary>
        public string SortKeyAttribute { get; set; }
    }

    /// <summary>
    /// Data structure to represent provisioned throughput.
    /// Used at table or index level.
    /// </summary>
    public class ProvisionedThroughput
    {
        /// <summary>
        /// Read Capacity Units (RCU).
        /// If Capacity Mode is set to PAY_PER_REQUEST, this value is set to 0.
        /// </summary>
        public long ReadCapacityUnits { get; set; }

        /// <summary>
        /// Write Capacity Units (WCU).
        /// If Capacity Mode is set to PAY_PER_REQUEST, this value is set to 0.
        /// </summary>
        public long WriteCapacityUnits { get; set; }
    }

    /// <summary>
    /// Data structure to represent index projection settings.
    /// </summary>
    public class IndexProjection
    {
        /// <summary>
        /// List of attribute names to be projected into the index.
        /// </summary>
        public List<string> AttributeNames { get; set; } = new List<string>();

        /// <summary>
        /// Index projection type.
        /// </summary>
        public IndexProjectionType ProjectionType { get; set; }
    }

    /// <summary>
    /// Data structure to represent the Time-To-Live configuration.
    /// </summary>
    public class TimeToLiveConfig
    {
        /// <summary>
        /// Name of the attribute.
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Flag to inndicate whether Time-To-Live is enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Enumeration to represent capacity modes.
    /// </summary>
    public enum CapacityMode
    {
        /// <summary>
        /// On-demand, pay-per-request capacity mode.
        /// Billed by request count.
        /// Recommended for unpredictable workloads.
        /// Preferable to begin with application development with DynamoDB as database.
        /// </summary>
        PAY_PER_REQUEST,

        /// <summary>
        /// Provisioned, pay-per-second billing mode.
        /// Billed by capacity units consumed per second.
        /// Recommended for predictable workloads.
        /// </summary>
        PROVISIONED
    }

    /// <summary>
    /// Enumeration to represent DynamoDb Server Side Encryption types
    /// </summary>
    public enum EncryptionType
    {
        /// <summary>
        /// KMS encryption.
        /// </summary>
        KMS
    }

    /// <summary>
    /// Enumeration to represent types of index projection.
    /// </summary>
    public enum IndexProjectionType
    {
        /// <summary>
        /// All table attributes will be projected into the index.
        /// </summary>
        ALL,

        /// <summary>
        /// Specified attributes will be projected into the index.
        /// </summary>
        INCLUDE,

        /// <summary>
        /// Only primary key/sort key attributes are projected into the index.
        /// </summary>
        KEYS_ONLY
    }

    /// <summary>
    /// Enumeration to represent DynamoDB table class.
    /// </summary>
    public enum TableClass
    {
        /// <summary>
        /// Standard table.
        /// </summary>
        STANDARD,

        /// <summary>
        /// Standard-Infrequent-Access table.
        /// </summary>
        STANDARD_INFREQUENT_ACCESS
    }
}
