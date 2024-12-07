

namespace NoSqlDataAccess.Common.Constants
{
    /// <summary>
    /// This class provides CosmosDB Operators
    /// </summary>
    public static class DBOperators
    {
        /// <summary>
        /// CosmosDB update operators.
        /// </summary>
        public static class Update
        {
            /// <summary>
            /// ADD Operator.
            /// Adds the specified value to the item, if the attribute does not already exist. If the attribute does exist, then the behavior of ADD depends on the data type of the attribute:
            ///     If the existing attribute is a number, and if Value is also a number, then Value is mathematically added to the existing attribute.If Value is a negative number, then it is subtracted from the existing attribute.
            ///     If the existing data type is a set and if Value is also a set, then Value is added to the existing set. For example, if the attribute value is the set [1,2], and the ADD action specified [3], then the final attribute value is [1,2,3]. An error occurs if an ADD action is specified for a set attribute and the attribute type specified does not match the existing set type.
            ///         Both sets must have the same primitive data type.For example, if the existing data type is a set of strings, the Value must also be a set of strings.
            /// IMPORTANT: The ADD action only supports Number and set data types. In addition, ADD can only be used on top-level attributes, not nested attributes.
            /// </summary>
            public const string ADD = "ADD";

            /// <summary>
            /// DELETE Operator.
            /// Deletes an element from a set.
            ///    If a set of values is specified, then those values are subtracted from the old set.For example, if the attribute value was the set[a, b, c] and the DELETE action specifies[a, c], then the final attribute value is [b]. Specifying an empty set is an error.
            /// IMPORTANT: The DELETE action only supports set data types. In addition, DELETE can only be used on top-level attributes, not nested attributes.
            ///    You can have many actions in a single expression, such as the following: SET a =:value1, b=:value2 DELETE :value3, :value4, :value5
            /// </summary>
            public const string DELETE = "DELETE";

            /// <summary>
            /// REMOVE Operator.
            /// Removes one or more attributes from an item.
            /// </summary>
            public const string REMOVE = "REMOVE";

            /// <summary>
            /// SET Operator.
            /// Adds one or more attributes and values to an item. 
            /// If any of these attributes already exist, they are replaced by the new values. 
            /// You can also use SET to add or subtract from an attribute that is of type Number. For example: SET myNum = myNum + :val
            /// SET supports the following functions:
            ///     if_not_exists(path, operand) - if the item does not contain an attribute at the specified path, then if_not_exists evaluates to operand; otherwise, it evaluates to path.You can use this function to avoid overwriting an attribute that may already be present in the item.
            ///     list_append (operand, operand) - evaluates to a list with a new element added to it.You can append the new element to the start or the end of the list by reversing the order of the operands.
            /// These function names are case-sensitive.
            /// </summary>
            public const string SET = "SET";
        }

        /// <summary>
        /// CosmosDB Comparison operators.
        /// </summary>
        public static class Comparison
        {
            /// <summary>
            /// Equality Operator.
            /// </summary>
            public const string EQ = "=";

            /// <summary>
            /// Non-Equality Operator.
            /// </summary>
            public const string NEQ = "<>";

            /// <summary>
            /// Greater-Than Operator.
            /// </summary>
            public const string GT = ">";

            /// <summary>
            /// Greater-Than-Or-Equal Operator.
            /// </summary>
            public const string GTE = ">=";

            /// <summary>
            /// Less-Than Operator.
            /// </summary>
            public const string LT = "<";

            /// <summary>
            /// Less-Than-Or-Equal Operator.
            /// </summary>
            public const string LTE = "<=";

            /// <summary>
            /// BETWEEN Operator.
            /// </summary>
            public const string BETWEEN = "BETWEEN";

            /// <summary>
            /// IN Operator.
            /// </summary>
            public const string IN = "IN";

            /// <summary>
            /// contains Operator.
            /// </summary>
            public const string CONTAINS = "contains";
        }

        /// <summary>
        /// CosmosDB Logical operators.
        /// </summary>
        public static class Logical
        {
            /// <summary>
            /// AND Operator.
            /// </summary>
            public const string AND = "AND";

            /// <summary>
            /// OR Operator.
            /// </summary>
            public const string OR = "OR";

            /// <summary>
            /// NOT operator.
            /// </summary>
            public const string NOT = "NOT";
        }

        /// <summary>
        /// CosmosDB Function operators.
        /// </summary>
        public static class Function
        {
            /// <summary>
            /// 'attribute_exists' Function.
            /// </summary>
            public const string ATTRIBUTE_EXISTS = "attribute_exists";

            /// <summary>
            /// 'attribute_not_exists' Function.
            /// </summary>
            public const string ATTRIBUTE_NOT_EXISTS = "attribute_not_exists";
        }
    }
}
