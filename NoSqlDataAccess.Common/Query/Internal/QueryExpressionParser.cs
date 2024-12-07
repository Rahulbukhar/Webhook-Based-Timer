

using Newtonsoft.Json.Linq;
using NoSqlDataAccess.Common.Exceptions;
using NoSqlDataAccess.Common.Extensions;
using NoSqlDataAccess.Common.Query.Model;
using System;
using System.Collections.Generic;

namespace NoSqlDataAccess.Common.Query
{
    /// <summary>
    /// Parser for <see cref="SearchExpression"/>
    /// </summary>
    public static class QueryExpressionParser
    {
        /// <summary>
        /// Parses JSON data into SearchExpression.
        /// </summary>
        /// <param name="jsonData">JSON search expression input.</param>
        /// <returns>An instance of <see cref="SearchExpression"/>.</returns>
        public static SearchExpression Parse(JToken jsonData)
        {
            if (!jsonData.IsNullOrEmpty())
            {
                var op = ToEnum(jsonData["op"].Value<string>());
                switch (op)
                {
                    case QueryOperator.And:
                    case QueryOperator.Or:
                        var parsedSearchTerm = CheckQueryOpearatorOr(jsonData);
                        if (op == QueryOperator.And)
                        {
                            return new AndSearchExpression(parsedSearchTerm.ToArray());
                        }
                        else
                        {
                            return new OrSearchExpression(parsedSearchTerm.ToArray());
                        }
                    case QueryOperator.Eq:
                    case QueryOperator.Ne:
                    case QueryOperator.Lt:
                    case QueryOperator.Lte:
                    case QueryOperator.Gt:
                    case QueryOperator.Gte:
                    case QueryOperator.In:
                    case QueryOperator.Between:
                        return CheckQueryOpearatorNotInArray(jsonData, op);
                    default:
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts QueryTerm and parse it to related datatypes.
        /// </summary>
        /// <param name="json">The JSON input.</param>
        /// <returns>An instance of <see cref="QueryTerm"/>.</returns>
        private static QueryTerm ConvertToQueryTerm(JToken json)
        {
            QueryTerm expression = QueryTerm.Null;
            if (json.Type == JTokenType.Array)
            {
                if (!json.HasValues)
                {
                    return expression;
                }

                switch (json[0].Type)
                {
                    case JTokenType.Integer:
                        {
                            expression = json.ToObject<long[]>();
                        }
                        break;
                    case JTokenType.Float:
                        {
                            expression = json.ToObject<double[]>();
                        }
                        break;
                    case JTokenType.String:
                        {
                            expression = json.ToObject<string[]>();
                        }
                        break;
                    case JTokenType.Boolean:
                        {
                            expression = json.ToObject<bool[]>();
                        }
                        break;
                    case JTokenType.Null:
                        {
                            expression = QueryTerm.Null;
                        }
                        break;
                    case JTokenType.Date:
                        {
                            expression = CheckQueryOpearatorDateWithJTokenArray(json);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (json.Type)
                {
                    case JTokenType.Integer:
                        {
                            expression = json.ToObject<long>();
                        }
                        break;
                    case JTokenType.Float:
                        {
                            expression = json.ToObject<double>();
                        }
                        break;
                    case JTokenType.String:
                        {
                            expression = json.ToObject<string>();
                        }
                        break;
                    case JTokenType.Boolean:
                        {
                            expression = json.ToObject<bool>();
                        }
                        break;
                    case JTokenType.Null:
                        {
                            expression = QueryTerm.Null;
                        }
                        break;
                    case JTokenType.Date:
                        expression = CheckQueryOpearatorDate(json);
                        break;
                    default:
                        break;
                }
            }
            return expression;
        }

        /// <summary>
        /// Converts input string to <see cref="QueryOperator"/>.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <returns><see cref="QueryOperator"/> enumeration value.</returns>
        private static QueryOperator ToEnum(string value)
        {
            var dataType = (QueryOperator)Enum.Parse(typeof(QueryOperator), value);
            return dataType;
        }

        /// <summary>
        /// Validates OR query operator.
        /// </summary>
        /// <param name="jsonData">Input JSON.</param>
        /// <returns>An instance of <see cref="List{SearchExpression}"/>.</returns>
        /// <exception cref="DataAccessException">Thrown when search term value is not an array.</exception>
        private static List<SearchExpression> CheckQueryOpearatorOr(JToken jsonData)
        {
            var jsonSearchTerms = jsonData["searchTerms"];
            if (jsonSearchTerms.Type != JTokenType.Array)
            {
                throw new DataAccessException("Invalid Search Criteria!");
            }
            List<SearchExpression> parsedSearchTerm = new List<SearchExpression>();
            foreach (var jsonSearchTerm in jsonSearchTerms)
            {
                SearchExpression searchTerm = Parse(jsonSearchTerm);
                parsedSearchTerm.Add(searchTerm);
            }
            return parsedSearchTerm;
        }

        /// <summary>
        /// Parses IN query operator with non-array values.
        /// </summary>
        /// <param name="jsonData">Input JSON.</param>
        /// <param name="op">Query operator.</param>
        /// <returns></returns>
        private static SearchExpression CheckQueryOpearatorNotInArray(JToken jsonData, QueryOperator op)
        {
            QueryTerm left, right;
            left = jsonData["left"].Value<string>();
            right = ConvertToQueryTerm(jsonData["right"]);
            return new BinarySearchExpression(op, left, right);
        }

        /// <summary>
        /// Formats date value in a query opertor into universal time.
        /// </summary>
        /// <param name="jsonData">Input JSON.</param>
        /// <returns>An instacnce of <see cref="QueryTerm"/>.</returns>
        private static QueryTerm CheckQueryOpearatorDate(JToken jsonData)
        {
            var value = jsonData.Value<DateTime>();
            if (value.Kind != DateTimeKind.Utc)
            {
                value = value.ToUniversalTime();
            }
            QueryTerm expression = value;
            return expression;
        }

        /// <summary>
        /// Parses query date array value.
        /// </summary>
        /// <param name="jsonData">Input JSON.</param>
        /// <returns>An instacnce of <see cref="QueryTerm"/>.</returns>
        private static QueryTerm CheckQueryOpearatorDateWithJTokenArray(JToken jsonData)
        {
            var values = new JArray();
            foreach (var item in JArray.FromObject(jsonData))
            {
                if (item != null)
                {
                    if (item.Value<DateTime>().Kind != DateTimeKind.Utc)
                    {
                        values.Add(item.Value<DateTime>().ToUniversalTime());
                    }
                    else
                    {
                        values.Add(item.Value<DateTime>());
                    }
                }
            }
            QueryTerm expression = values.ToObject<DateTime[]>();
            return expression;
        }
    }
}
