

namespace NoSqlDataAccess.Common.Constants
{
    /// <summary>
    /// CosmosDB constants.
    /// </summary>
    public static class DbConstants
    {
        /// <summary>
        /// Constant for default pagination limit for batch-get-item or query operation.
        /// </summary>
        public const int BATCH_GET_OR_QUERY_PAGINATION_LIMIT_DEFAULT = 100;

        /// <summary>
        /// Constant for maximum value of limit when using pagination in query operation.
        /// </summary>
        public const int QUERY_PAGINATION_LIMIT_MAX = 500;

        /// <summary>
        /// Constant for default value of skip when using pagination in query operation.
        /// </summary>
        public const int QUERY_PAGINATION_SKIP_DEFAULT = 0;

        /// <summary>
        /// Constant for maximum count of inputs for batch-write operation.
        /// </summary>
        public const int BATCH_WRITE_MAX_INPUT_OBJECTS_COUNT = 25;

        /// <summary>
        /// Constant for maximum number of objects to bulk delete.
        /// </summary>
        public const int BULK_DELETE_MAX_OBJECTS_COUNT = 500;

        /// <summary>
        /// Constant for maximum number of objects to clone.
        /// </summary>
        public const int CLONE_MAX_OBJECTS_COUNT_PER_TABLE = 500;

        /// <summary>
        /// Constant 'PrimaryKey'.
        /// </summary>
        public const string KEY_TYPE_PRIMARY = "PrimaryKey";

        /// <summary>
        /// Constant 'SortKey'.
        /// </summary>
        public const string KEY_TYPE_SORT = "SortKey";

        /// <summary>
        /// Constant 'Date time format'.
        /// </summary>
        public const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Constant 'EMPTY_OBJECT'.
        /// </summary>
        public const string EMPTY_OBJECT = "EMPTY_OBJECT";

        /// <summary>
        /// Constant 'EMPTY_LIST'.
        /// </summary>
        public const string EMPTY_LIST = "EMPTY_LIST";

        /// <summary>
        /// Constant 'Object'.
        /// </summary>
        public const string OBJECT = "Object";
    }
}
