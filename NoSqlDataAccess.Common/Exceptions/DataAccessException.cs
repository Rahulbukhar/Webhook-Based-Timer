

using System;

namespace NoSqlDataAccess.Common.Exceptions
{
#pragma warning disable S3925

    /// <summary>
    /// Exception class for handling exceptions raised by DataAccess Service.
    /// </summary>
    [Serializable]
    public class DataAccessException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="DataAccessException"/>
        /// </summary>
        public DataAccessException() { }

        /// <summary>
        /// Exception for handling the search criteria.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public DataAccessException(string message) : base(message) { }

        /// <summary>
        /// Exception for handling the search criteria.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">Originated exception (optional).</param>
        public DataAccessException(string message, Exception inner) : base(message, inner) { }
    }

#pragma warning restore S3925
}
