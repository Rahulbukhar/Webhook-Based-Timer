

using Newtonsoft.Json.Linq;

namespace NoSqlDataAccess.Common.Logging
{
#pragma warning disable S3925
    /// <summary>
    /// Utility class format logger messages for performance logs.
    /// </summary>
    public class DbPerformanceLogFormatter
    {
        /// <summary>
        /// Object to be logged.
        /// </summary>
        private readonly JObject loggingObject;

        /// <summary>
        /// Describes the operation we're currently trying to do.
        /// </summary>
        public enum Operation
        {
            /// <summary>
            /// Query operation
            /// </summary>
            Query = 0,

            /// <summary>
            /// Insert operation
            /// </summary>
            Insert,

            /// <summary>
            /// Update operation
            /// </summary>
            Update,

            /// <summary>
            /// Delete operation
            /// </summary>
            Delete,

            /// <summary>
            /// Clone operation
            /// </summary>
            Clone
        };

        /// <summary>
        /// Constructor for logging formatter class
        /// </summary>
        /// <param name="table">Name of the table being impacted.</param>
        /// <param name="op">Type of the operation.</param>
        /// <param name="duration">Operation duration in ms.</param>
        /// <param name="objects">Number of objects impacted or returned.</param>
        public DbPerformanceLogFormatter(string table, Operation op, long duration, long objects)
        {
            loggingObject = new JObject
            {
                { "messageType", "Performance Metric" },
                { "operation", ConvertOperationToString(op) },
                { "table", table },
                { "duration", duration },
                { "numberOfObjects", objects }
            };
        }

        /// <summary>
        /// Allows setting of additional information to be logged.
        /// </summary>
        /// <param name="name">Name of this piece of information</param>
        /// <param name="value">Value to be recorded. Must be a JSON compatible type</param>
        public void AddLoggingInfo(string name, JToken value)
        {
            loggingObject.Add(name, value);
        }

        /// <summary>
        /// Converts the object to a string with a standard format
        /// </summary>
        /// <returns>Formatted string</returns>
        public override string ToString()
        {
            return loggingObject.ToString();
        }

        /// <summary>
        /// Get string equivalent of the Operation
        /// </summary>
        /// <param name="op">Type of the operation</param>
        /// <returns>Returns the string equivalent of the Operation</returns>
        public static string ConvertOperationToString(Operation op)
        {
            switch (op)
            {
                case Operation.Query:
                    return "Query";
                case Operation.Insert:
                    return "Insert";
                case Operation.Delete:
                    return "Delete";
                case Operation.Update:
                    return "Update";
            }
            return "Unknown";
        }
    }
#pragma warning restore S3925
}
