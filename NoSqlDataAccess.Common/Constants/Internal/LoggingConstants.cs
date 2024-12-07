

namespace NoSqlDataAccess.Common.Constants.Internal
{
    /// <summary>
    /// Logging constants.
    /// </summary>
    public static class LoggingConstants
    {
        /// <summary>
        /// Constant for 'count'.
        /// </summary>
        public const string COUNT = "count";

        /// <summary>
        /// Constant for 'cosmosdb'.
        /// </summary>
        public const string COSMOSDB = "cosmosdb";

        /// <summary>
        /// Constant for 'dynamodb'.
        /// </summary>
        public const string DYNAMODB = "dynamodb";

        /// <summary>
        /// Constant for 'cosmosdb.{operationType}.capacityUnits.{tableName}.count'.
        /// </summary>
        public const string GENERIC_CAPACITY_UNITS_CONSUMED_METRIC = COSMOSDB + ".{0}" + ".capacityUnits.{1}." + COUNT;

        /// <summary>
        /// Constant for 'cosmosdb.{operationType}.readCapacityUnits.{tableName}.count'.
        /// </summary>
        public const string READ_CAPACITY_UNITS_CONSUMED_METRIC = COSMOSDB + ".{0}" + ".readCapacityUnits.{1}." + COUNT;

        /// <summary>
        /// Constant for 'cosmosdb.{operationType}.writeCapacityUnits.{tableName}.count'.
        /// </summary>
        public const string WRITE_CAPACITY_UNITS_CONSUMED_METRIC = COSMOSDB + ".{0}" + ".writeCapacityUnits.{1}." + COUNT;

        /// <summary>
        /// Constant for 'cosmosdb.provisionedThroughputExceeded.count'.
        /// </summary>
        public const string PROVISIONED_THROUGHPUT_EXCEEDED_METRIC = COSMOSDB + ".provisionedThroughputExceeded." + COUNT;

        /// <summary>
        /// Constant for 'cosmosdb.unProcessedItems.{tableName}.count'.
        /// </summary>
        public const string GENERIC_UNPROCESSED_ITEMS_METRIC = COSMOSDB + ".unProcessedItems.{0}." + COUNT;

        /// <summary>
        /// Constant for 'tableName'
        /// </summary>
        public const string TABLENAME = "tableName";

        /// <summary>
        /// Constant for 'remote'. 
        /// </summary>
        public const string REMOTE = "remote";

        /// <summary>
        /// Constant for 'Milliseconds'.
        /// </summary>
        public const string MILLISECONDS = "Milliseconds";

        /// <summary>
        /// Constant for 'duration'.
        /// </summary>
        public const string DURATION = "duration";

        /// <summary>
        /// Constant for 'db.time'.
        /// </summary>
        public const string DB_TIME_METRIC = "db.time";

        /// <summary>
        /// Constant for 'db.count'.
        /// </summary>
        public const string DB_COUNT_METRIC = "db.count";

        /// <summary>
        /// Constant for 'db.nested.time'.
        /// </summary>
        public const string DB_NESTED_TIME_METRIC = "db.nested.time";

        /// <summary>
        /// Constant for 'db.nested.count'.
        /// </summary>
        public const string DB_NESTED_COUNT_METRIC = "db.nested.count";
    }
}
