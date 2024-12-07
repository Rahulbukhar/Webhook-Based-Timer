

using NoSqlDataAccess.Common.Constants.Internal;
using NoSqlDataAccess.Common.Provisioning.Model;
using NoSqlDataAccess.Common.Utility;
using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Provisioning.Internal
{
    /// <summary>
    /// This class provides configuration(s) of DynamoDB tables used by default.
    /// </summary>
    internal static class DefaultTableConfiguration
    {
        /// <summary>
        /// Returns configuration of the table that stores query pagination tokens.
        /// </summary>
        public static TableConfiguration CreateQueryTokenTableConfiguration(string tableName)
        {
            return new TableConfiguration
            {
                CapacityMode = CapacityMode.PAY_PER_REQUEST,
                DeleteProtectionEnabled = true,
                EncryptionConfiguration = null,
                ProvisionedThroughput = null,
                Schema = new TableSchema
                {
                    Attributes = new List<TableAttribute>
                    {
                        DbUtil.ConstructTableAttribute(QueryTokenConstants.Attributes.EXPIRATION, typeof(long)),
                        DbUtil.ConstructTableAttribute(QueryTokenConstants.Attributes.LAST_EVALUATED_KEY, typeof(object)),
                        DbUtil.ConstructTableAttribute(QueryTokenConstants.Attributes.PAGINATION_HASH_KEY, typeof(string)),
                        DbUtil.ConstructTableAttribute(QueryTokenConstants.Attributes.USER_ID, typeof(string))
                    },
                    KeyConfiguration = new KeyConfiguration
                    {
                        PrimaryKeyAttribute = QueryTokenConstants.Attributes.USER_ID,
                        SortKeyAttribute = QueryTokenConstants.Attributes.PAGINATION_HASH_KEY
                    }
                },
                TableName = tableName,
                TableClass = TableClass.STANDARD,
                Tags = new Dictionary<string, string>
                {
                    { "purpose", "Stores query pagination tokens" }
                },
                TimeToLiveConfig = new TimeToLiveConfig
                {
                    AttributeName = QueryTokenConstants.Attributes.EXPIRATION,
                    Enabled = true
                }
            };
        }        
    }
}
