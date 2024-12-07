

using NoSqlDataAccess.Common.Constants;
using NoSqlDataAccess.Common.Constants.Internal;
using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Query.Model
{
    /// <summary>
    /// This class represents options available for finding documents from a document database.
    /// </summary>
    public class FindOptions
    {
        /// <summary>
        /// Sort field.
        /// </summary>
        public SortField SortField { get; set; }

        /// <summary>
        /// List of fields to be projected.
        /// All properties are returned by default.
        /// </summary>
        public List<string> ProjectionFields { get; set; } = CommonConstants.DEFAULT_PROJECTION_PROPERTIES_ALL;

        /// <summary>
        /// Number of items to return.
        /// </summary>
        public int? Limit { get; set; } = DbConstants.BATCH_GET_OR_QUERY_PAGINATION_LIMIT_DEFAULT;

        /// <summary>
        /// Number of items to skip.
        /// </summary>
        public int? Skip { get; set; } = DbConstants.QUERY_PAGINATION_SKIP_DEFAULT;
    }

    /// <summary>
    /// This class represents a sort field and its sort direction.
    /// </summary>
    public class SortField
    {
        /// <summary>
        /// The name of the attribute on which sorting needs to be done.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Use Ascending sort direction for the field.
        /// </summary>
        public SortDirection SortDirection { get; set; }
    }

    /// <summary>
    /// Enumeration to capture sort directions
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ascending sort direction.
        /// </summary>
        Ascending = 1,

        /// <summary>
        /// Descending sort direction.
        /// </summary>
        Descending = -1
    }
}
