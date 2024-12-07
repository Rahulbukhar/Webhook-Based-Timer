

using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Model
{
    /// <summary>
    /// Data structure to represent status of clone operation on the table.
    /// </summary>
    public class CloneTableStatus
    {
        /// <summary>
        /// Name of the source table.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Clone status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Count of items queried from table.
        /// </summary>
        public long QueriedItemCount { get; set; }

        /// <summary>
        /// Count of items cloned from table.
        /// </summary>
        public long ClonedItemCount { get; set; }

        /// <summary>
        /// Map of attribute values from last evaluation.
        /// </summary>
        public Dictionary<string, object> LastEvaluatedAttributes { get; set; }
    }

    /// <summary>
    /// This class provides constants for <see cref="CloneTableStatus"/>.
    /// </summary>
    public static class CloneTableStatusConstants
    {
        /// <summary>
        /// Constant for 'Completed'.
        /// </summary>
        public const string COMPLETED = "Completed";

        /// <summary>
        /// Constant for 'Processing'.
        /// </summary>
        public const string PROCESSING = "Processing";

        /// <summary>
        /// Constant for 'Progressing'.
        /// </summary>
        public const string PROGRESSING = "Progressing";

        /// <summary>
        /// Constant for 'Failed'.
        /// </summary>
        public const string FAILED = "Failed";
    }
}
