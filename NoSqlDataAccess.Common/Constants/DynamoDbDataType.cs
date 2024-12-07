

namespace NoSqlDataAccess.Common.Constants.Internal
{
    /// <summary>
    /// This class provides data types specific to Common attribute values.
    /// </summary>
    public static class DynamoDbDataType
    {
        /// <summary>
        /// DynamoDB Boolean.
        /// </summary>
        public const string BOOLEAN = "BOOL";

        /// <summary>
        /// DynamoDB Number.
        /// </summary>
        public const string NUMBER = "N";

        /// <summary>
        /// DynamoDB String.
        /// </summary>
        public const string STRING = "S";

        /// <summary>
        /// DynamoDB List.
        /// </summary>
        public const string LIST = "L";

        /// <summary>
        /// DynamoDB Map.
        /// </summary>
        public const string MAP = "M";
    }
}
