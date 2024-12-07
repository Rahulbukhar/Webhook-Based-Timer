

using System;

namespace NoSqlDataAccess.Common.Exceptions
{
#pragma warning disable S3925

    /// <summary>
    /// Exception to be thrown when the table is not found.
    /// </summary>
    public class TableNotFoundException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="TableNotFoundException"/>
        /// </summary>
        public TableNotFoundException() { }

        /// <summary>
        /// Initializes an instance of <see cref="TableNotFoundException"/> with a message
        /// </summary>
        public TableNotFoundException(string message) : base(message) { }

        /// <summary>
        /// Initializes an instance of <see cref="TableNotFoundException"/> with a message and an inner exception
        /// </summary>
        public TableNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

#pragma warning restore S3925
}
