

using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Constants.Internal
{
    /// <summary>
    /// Commo constants.
    /// </summary>
    public static class CommonConstants
    {
        /// <summary>
        /// List of default projection properties.
        /// All properties will be returned by default.
        /// </summary>
        public static readonly List<string> DEFAULT_PROJECTION_PROPERTIES_ALL = new List<string> { WILDCARD_ASTERISK }; // NOSONAR

        /// <summary>
        /// A reference empty list.
        /// Helps to avoid re-initialization in recursive calls.
        /// </summary>
        public static readonly List<string> EMPTY_LIST = new List<string>(); // NOSONAR

        /// <summary>
        /// Constant for '.'.
        /// </summary>
        public const string DOT = ".";

        /// <summary>
        /// Constant for '/'.
        /// </summary>
        public const string FWD_SLASH_SEPARATOR = "/";

        /// <summary>
        /// Constant for '{'.
        /// </summary>
        public const string CURLY_BRACKET_OPEN = "{";

        /// <summary>
        /// Constant for '}'.
        /// </summary>
        public const string CURLY_BRACKET_CLOSE = "}";

        /// <summary>
        /// Constant for '('.
        /// </summary>
        public const string ROUND_BRACKET_OPEN = "(";

        /// <summary>
        /// Constant for ')'.
        /// </summary>
        public const string ROUND_BRACKET_CLOSE = ")";

        /// <summary>
        /// Constant for '*'.
        /// </summary>
        public const string WILDCARD_ASTERISK = "*";

        /// <summary>
        /// Constant for ','.
        /// </summary>
        public const string CSV_SEPARATOR = ",";
        
        /// <summary>
        /// Constant for ';'.
        /// </summary>
        public const string SEMICOLON= ";";

        /// <summary>
        /// Constant for '#'.
        /// Used to prefix the attribute name key.
        /// </summary>
        public const string EXPRESSION_ATTR_NAME_KEY_PREFIX = "#";

        /// <summary>
        /// Constant for ':'.
        /// Used to prefix the attribute value key.
        /// </summary>
        public const string EXPRESSION_ATTR_VALUE_KEY_PREFIX = ":";

        /// <summary>
        /// Constant for 'String[]'.
        /// </summary>
        public const string TYPE_NAME_STRING_ARRAY = "String[]";

        /// <summary>
        /// Constant for 'projectionAttributes'.
        /// </summary>
        public const string PROJECTION_ATTRIBUTES = "projectionAttributes";

        /// <summary>
        /// Constant for 'localhost'.
        /// </summary>
        public const string LOCALHOST = "localhost";

        /// <summary>
        /// Constant for 'isDeleted'.
        /// </summary>
        public const string IS_DELETED = "isDeleted";

        /// <summary>
        /// Constant for 'purgeDeletedAfterDays'.
        /// </summary>
        public const string PURGEDELETEDAFTERDAYS = "purgeDeletedAfterDays";

        /// <summary>
        /// Constant for 'ttl'.
        /// </summary>
        public const string TTL = "ttl";
    }
}
