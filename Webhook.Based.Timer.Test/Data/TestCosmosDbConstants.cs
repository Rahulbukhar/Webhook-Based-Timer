

namespace NoSqlDataAccess.Azure.CosmosDB.Test.Constants
{
    /// <summary>
    /// CosmosDB constants.
    /// </summary>
    public static class TestCosmosDbConstants
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
        /// Constant for maximum number of objects to bulk delete per iteration.
        /// </summary>
        public const int BULK_DELETE_MAX_OBJECTS_COUNT_PER_ITERATION = 500;

        /// <summary>
        /// Constant for maximum number of objects to clone.
        /// </summary>
        public const int CLONE_MAX_OBJECTS_COUNT_PER_TABLE = 500;

        /// <summary>
        /// Constant 'PrimaryKey'.
        /// </summary>
        public const string KEY_TYPE_PRIMARY = "PrimaryKey";

        /// <summary>
        /// Constant for region key.
        /// </summary>
        public const string REGION_KEY = "REGION";

        /// <summary>
        /// Is deleted flag name.
        /// </summary>
        public const string IS_DELETED = "isDeleted";

        /// <summary>
        /// Service name.
        /// </summary>
        public const string SERVICE_NAME = "TEST_SERVICE";

        /// <summary>
        /// Unique id.
        /// </summary>
        public const string UNIQUE_ID = "vaultId";

        /// <summary>
        /// id.
        /// </summary>
        public const string ID = "id";

        /// <summary>
        /// Version Id.
        /// </summary>
        public const string VERSION_ID = "versionId";

        /// <summary>
        /// Creation Date.
        /// </summary>
        public const string CREATION_DATE = "creationDate";


        /// <summary>
        /// Last modified date name.
        /// </summary>
        public const string LAST_MODIFIED_DATE = "lastModifiedDate";

        /// <summary>
        /// Created by name.
        /// </summary>
        public const string CREATED_BY = "createdBy";

        /// <summary>
        /// Last modified by name.
        /// </summary>
        public const string LAST_MODIFIED_BY = "lastModifiedBy";

        /// <summary>
        /// Constant for CosmosDB service URL.
        /// </summary>
        public const string COSMOSDB_SERVICE_URL_KEY = "COSMOSDB_SERVICE_URL";
    }
}
