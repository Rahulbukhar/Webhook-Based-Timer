

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NoSqlDataAccess.Common.Query.Model
{
    /// <summary>
    /// This class represents the query operators.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QueryOperator
    {
        /// <summary>
        /// Logical AND operator
        /// </summary>
        And,

        /// <summary>
        /// Logical OR operator
        /// </summary>
        Or,

        /// <summary>
        /// Logical NOT operator
        /// </summary>
        Not,

        /// <summary>
        /// Relational equality operator
        /// </summary>
        Eq,

        /// <summary>
        /// Relational inequality operator
        /// </summary>
        Ne,

        /// <summary>
        /// Relational Less than operator
        /// </summary>
        Lt,

        /// <summary>
        /// Relational Less than or equal to operator
        /// </summary>
        Lte,

        /// <summary>
        /// Relational greater than operator
        /// </summary>
        Gt,

        /// <summary>
        /// Relational greater than or equal to operator
        /// </summary>
        Gte,

        /// <summary>
        /// In Array operator 
        /// </summary>
        In,

        /// <summary>
        /// Comparison 'Between' operator 
        /// </summary>
        Between,

        /// <summary>
        /// Comparison 'Contains' operator 
        /// </summary>
        Contains
    }
}
