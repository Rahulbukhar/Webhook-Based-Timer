

using System;

namespace NoSqlDataAccess.Common.Exceptions
{
#pragma warning disable S3925

    /// <summary>
    /// Exception to be thrown when DB operation input contains a duplicate key.
    /// The key can be a primary key value supplied for any of the create/read/update/delete operation.
    /// </summary>
    public class DuplicateKeyException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="DuplicateKeyException"/>
        /// </summary>
        public DuplicateKeyException() { }

        /// <summary>
        /// Initializes an instance of <see cref="DuplicateKeyException"/> with a message
        /// </summary>
        public DuplicateKeyException(string message) : base(message) { }

        /// <summary>
        /// Initializes an instance of <see cref="DuplicateKeyException"/> with a message and an inner exception
        /// </summary>
        public DuplicateKeyException(string message, Exception inner) : base(message, inner) { }
    }

#pragma warning restore S3925
}
