

using System.ComponentModel;

namespace NoSqlDataAccess.Common.Query.Model
{
    /// <summary>
    /// This class respresents data types supported for query.
    /// </summary>
    public enum QueryDataType
    {
        /// <summary>
        /// Date type.
        /// </summary>
        [Description("Date")]
        Date,

        /// <summary>
        /// Double type.
        /// </summary>
        [Description("Double")]
        Double,

        /// <summary>
        /// Integer type. Supports Long integer.
        /// </summary>
        [Description("Integer")]
        Integer,

        /// <summary>
        /// Boolean type.
        /// </summary>
        [Description("Boolean")]
        Boolean,

        /// <summary>
        /// String type.
        /// </summary>
        [Description("String")]
        String,

        /// <summary>
        /// Unknown type.
        /// </summary>
        [Description("Unknown")]
        Unknown
    }
}
