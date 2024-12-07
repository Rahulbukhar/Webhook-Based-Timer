

using NoSqlDataAccess.Common.Constants.Internal;
using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Model
{
    /// <summary>
    /// This class provides options to return values from DynamoDB operations.
    /// </summary>
    public class ReturnValueOption
    {
        /// <summary>
        /// Option to return all new state of all attributes of the object.
        /// </summary>
        public static readonly ReturnValueOption ALL_NEW = new ReturnValueOption(ReturnValueConstants.ALL_NEW);

        /// <summary>
        /// Option to return old state of all attributes of the object.
        /// </summary>
        public static readonly ReturnValueOption ALL_OLD = new ReturnValueOption(ReturnValueConstants.ALL_OLD);

        /// <summary>
        /// Option to return old and new state of all attributes of the object.
        /// </summary>
        public static readonly ReturnValueOption ALL_OLD_NEW = new ReturnValueOption(ReturnValueConstants.ALL_OLD_NEW);

        /// <summary>
        /// Option to return none of the old or new state attributes of the object.
        /// </summary>
        public static readonly ReturnValueOption NONE = new ReturnValueOption(ReturnValueConstants.NONE);

        /// <summary>
        /// Option to return old and new state of attributes specified in the database operaion.
        /// </summary>
        public static readonly ReturnValueOption UPDATED_OLD_NEW = new ReturnValueOption(ReturnValueConstants.UPDATED_OLD_NEW);

        /// <summary>
        /// Option to return new state of attributes specified in the database operaion.
        /// </summary>
        public static readonly ReturnValueOption UPDATED_NEW = new ReturnValueOption(ReturnValueConstants.UPDATED_NEW);

        /// <summary>
        /// Option to return old state of attributes specified in the database operation.
        /// </summary>
        public static readonly ReturnValueOption UPDATED_OLD = new ReturnValueOption(ReturnValueConstants.UPDATED_OLD);

        /// <summary>
        /// Gets the return option value.
        /// </summary>
        public string Value { get { return optionValue; } }

        /// <summary>
        /// Gets the return the mapped option value.
        /// </summary>
        public string MappedOptionValue { get { return mappedOptionValue; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">The value of return option.</param>
        public ReturnValueOption(string value)
        {
            mappedOptionValue = value;
            if (customDynamoDbReturnValueMap.ContainsKey(value))
            {
                optionValue = customDynamoDbReturnValueMap[value];
            }
            else
            {
                optionValue = value;
            }
        }

        /// <summary>
        /// Holds the return value option.
        /// </summary>
        private string optionValue { get; }

        /// <summary>
        /// Holds the return value option.
        /// </summary>
        private string mappedOptionValue { get; }

        /// <summary>
        /// Map of custom return value options to DynamoDB return value option.
        /// The options in this map are not directly supported by DynamoDB. 
        /// </summary>
        private readonly Dictionary<string, string> customDynamoDbReturnValueMap = new Dictionary<string, string>
        {
            { ReturnValueConstants.ALL_OLD_NEW, ReturnValueConstants.ALL_OLD },
            { ReturnValueConstants.UPDATED_OLD_NEW, ReturnValueConstants.UPDATED_OLD }
        };
    }
}
