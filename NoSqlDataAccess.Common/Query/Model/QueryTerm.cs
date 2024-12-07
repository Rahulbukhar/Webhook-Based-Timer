


using System;
using System.Linq;
using System.Text;

namespace NoSqlDataAccess.Common.Query.Model
{
    /// <summary>
    /// This class represents the data structure for simple query condition composed of left and right operands and a relational or logical operator. 
    /// </summary>
    public class QueryTerm
    {
        private readonly QueryDataType dataType;
        private readonly bool isArray;
        private readonly bool isNull;
        private readonly object value;

        /// <summary>
        /// Constructor - Handles Data Type, Array, Booleans and Value.
        /// </summary>
        private QueryTerm(QueryDataType dataType, bool isArray, bool isNull, object value)
        {
            this.dataType = dataType;
            this.isArray = isArray;
            this.isNull = isNull;
            this.value = value;
        }

        /// <summary>
        /// Datatype of QueryTerm
        /// </summary>
        public QueryDataType DataType
        {
            get
            {
                return dataType;
            }
        }

        /// <summary>
        /// Whether QueryTerm is multi-valued
        /// </summary>
        public bool IsArray
        {
            get
            {
                return isArray;
            }
        }

        /// <summary>
        /// Whether QueryTerm is NULL.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return isNull;
            }
        }

        /// <summary>
        /// The underlying value of QueryTerm.
        /// </summary>
        public object Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Constant representing a NULL QueryTerm.
        /// </summary>
        public static QueryTerm Null
        {
            get
            {
                return new QueryTerm(QueryDataType.Unknown, false, true, null);
            }
        }

        /// <summary>
        /// Convert String Value to QueryTerm
        /// </summary>
        /// <param name="value">String value</param>
        public static implicit operator QueryTerm(string value)
        {
            return new QueryTerm(QueryDataType.String, false, false, value);
        }

        /// <summary>
        /// Converts Long Value to QueryTerm
        /// </summary>
        /// <param name="value">Long value</param>
        public static implicit operator QueryTerm(long value)
        {
            return new QueryTerm(QueryDataType.Integer, false, false, value);
        }

        /// <summary>
        /// Converts Double value to QueryTerm
        /// </summary>
        /// <param name="value">Double Value</param>
        public static implicit operator QueryTerm(double value)
        {
            return new QueryTerm(QueryDataType.Double, false, false, value);
        }

        /// <summary>
        /// Converts Boolean value to QueryTerm
        /// </summary>
        /// <param name="value">Boolean Value</param>
        public static implicit operator QueryTerm(bool value)
        {
            return new QueryTerm(QueryDataType.Boolean, false, false, value);
        }

        /// <summary>
        /// Converts String array to QueryTerm
        /// </summary>
        /// <param name="value">String Array</param>
        public static implicit operator QueryTerm(string[] value)
        {
            return new QueryTerm(QueryDataType.String, true, false, value);
        }

        /// <summary>
        /// Converts Long Array to QueryTerm
        /// </summary>
        /// <param name="value">Long Array</param>
        public static implicit operator QueryTerm(long[] value)
        {
            return new QueryTerm(QueryDataType.Integer, true, false, value);
        }

        /// <summary>
        /// Converts Double array to Queryterm
        /// </summary>
        /// <param name="value">Double Array</param>
        public static implicit operator QueryTerm(double[] value)
        {
            return new QueryTerm(QueryDataType.Double, true, false, value);
        }

        /// <summary>
        /// Converts Boolena Array to QueryTerm
        /// </summary>
        /// <param name="value">Boolean Array</param>
        public static implicit operator QueryTerm(bool[] value)
        {
            return new QueryTerm(QueryDataType.Boolean, true, false, value);
        }




        /// <summary>
        /// Overrided ToString Method.
        /// </summary>
        /// <returns>Formats Array input.</returns>
        public override string ToString()
        {
            if (isNull)
            {
                return "null";
            }

            if (isArray)
            {
                switch (dataType)
                {
                    case QueryDataType.Date:
                        var dateValues = (DateTime[])value;
                        return $"[{string.Join(", ", dateValues)}]";
                    case QueryDataType.Double:
                        var doubleValues = (double[])value;
                        return $"[{string.Join(", ", doubleValues)}]";
                    case QueryDataType.Integer:
                        var intValues = (long[])value;
                        return $"[{string.Join(", ", intValues)}]";
                    case QueryDataType.Boolean:
                        var boolValues = (bool[])value;
                        return $"[{string.Join(", ", boolValues)}]";
                    case QueryDataType.String:
                        var stringValues = ((string[])value).Select(val => $"\"{val}\"");
                        return $"[{string.Join(", ", stringValues)}]";
                    case QueryDataType.Unknown:
                    default:
                        break;
                }
                return string.Empty;
            }
            else
            {
                return $"{value}";
            }
        }

#pragma warning disable S3358 // Ternary operators should not be nested

        /// <summary>
        /// Converts QueryTerm to String
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator string(QueryTerm term)
        {
            var result = !term.IsArray
                ? term.DataType == QueryDataType.String
                ? (string)term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType} to string")
                : throw new InvalidCastException($"Cannot convert {term.dataType}[] to string");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Long.
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator long(QueryTerm term)
        {
            var result = !term.isArray
                ? term.DataType == QueryDataType.Integer
                ? (long)term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType} to integer")
                : throw new InvalidCastException($"Cannot convert {term.dataType}[] to integer");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Double
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator double(QueryTerm term)
        {
            var result = !term.isArray
                 ? term.DataType == QueryDataType.Double || term.DataType == QueryDataType.Integer
                 ? (double)term.value
                 : throw new InvalidCastException($"Cannot convert {term.dataType} to double")
                 : throw new InvalidCastException($"Cannot convert {term.dataType}[] to double");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Boolean
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator bool(QueryTerm term)
        {
            var result = !term.isArray
                ? term.DataType == QueryDataType.Boolean
                ? (bool)term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType} to boolean")
                : throw new InvalidCastException($"Cannot convert {term.dataType}[] to boolean");
            return result;
        }

        /// <summary>
        /// Converts Date value to QueryTerm
        /// </summary>
        /// <param name="value">DateTime Value</param>
        public static implicit operator QueryTerm(DateTime value)
        {
            return new QueryTerm(QueryDataType.Date, false, false, value);
        }

        /// <summary>
        /// Converts Date array value to QueryTerm
        /// </summary>
        /// <param name="value">DateTime Value</param>
        public static implicit operator QueryTerm(DateTime[] value)
        {
            return new QueryTerm(QueryDataType.Date, true, false, value);
        }

        /// <summary>
        /// Converts QueryTerm to String Array
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator string[](QueryTerm term)
        {
            var result = term.IsArray
                ? term.DataType == QueryDataType.String
                ? (string[])term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType}[] to string[]")
                : throw new InvalidCastException($"Cannot convert {term.dataType} to string[]");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Long array
        /// </summary>
        /// <param name="term">QueryTerm </param>
        public static implicit operator long[](QueryTerm term)
        {
            var result = term.isArray
                ? term.DataType == QueryDataType.Integer
                ? (long[])term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType}[] to integer[]")
                : throw new InvalidCastException($"Cannot convert {term.dataType} to integer[]");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Double Array
        /// </summary>
        /// <param name="term">Double Array</param>
        public static implicit operator double[](QueryTerm term)
        {
            var result = term.isArray
                ? term.DataType == QueryDataType.Double
                ? (double[])term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType}[] to double[]")
                : throw new InvalidCastException($"Cannot convert {term.dataType} to double[]");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Boolean Array
        /// </summary>
        /// <param name="term">Boolean Array</param>
        public static implicit operator bool[](QueryTerm term)
        {
            var result = term.isArray
               ? term.DataType == QueryDataType.Boolean
               ? (bool[])term.value
               : throw new InvalidCastException($"Cannot convert {term.dataType}[] to boolean[]")
               : throw new InvalidCastException($"Cannot convert {term.dataType} to boolean[]");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Date
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator DateTime(QueryTerm term)
        {
            var result = !term.isArray
                ? term.DataType == QueryDataType.Date
                ? (DateTime)term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType} to date")
                : throw new InvalidCastException($"Cannot convert {term.dataType} to date");
            return result;
        }

        /// <summary>
        /// Convert QueryTerm to Date Array
        /// </summary>
        /// <param name="term">QueryTerm</param>
        public static implicit operator DateTime[](QueryTerm term)
        {
            var result = term.isArray
                ? term.DataType == QueryDataType.Date
                ? (DateTime[])term.value
                : throw new InvalidCastException($"Cannot convert {term.dataType} to date")
                : throw new InvalidCastException($"Cannot convert {term.dataType} to date");
            return result;
        }
#pragma warning restore S3358 // Ternary operators should not be nested

    }
}
