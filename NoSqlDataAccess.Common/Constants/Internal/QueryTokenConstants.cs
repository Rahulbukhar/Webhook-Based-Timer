

namespace NoSqlDataAccess.Common.Constants.Internal
{
    /// <summary>
    /// This class provides constants related to query token table.
    /// </summary>
    public static class QueryTokenConstants
    {
        /// <summary>
        /// This class provides constants for query token table.
        /// </summary>
        internal static class Table
        {
            /// <summary>
            /// Constant for default name of the query tokens table.
            /// </summary>
            public const string NAME = "QueryTokens";
        }

        /// <summary>
        /// This class provides constants for query token attributes.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Constant for 'expiration' attribute.
            /// </summary>
            public const string EXPIRATION = "expiration";

            /// <summary>
            /// Constant for 'lastEvaluatedKey' attribute.
            /// </summary>
            public const string LAST_EVALUATED_KEY = "lastEvaluatedKey";

            /// <summary>
            /// Constant for 'paginationHashKey' attribute.
            /// </summary>
            public const string PAGINATION_HASH_KEY = "paginationHashKey";

            /// <summary>
            /// Constant for 'userId' attribute.
            /// </summary>
            public const string USER_ID = "userId";
        }
    }
}
