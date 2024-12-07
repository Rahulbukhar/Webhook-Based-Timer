

using Newtonsoft.Json.Linq;
using NoSqlDataAccess.Common.Query;
using NoSqlDataAccess.Common.Query.Model;
using System;

namespace NoSqlDataAccess.Common.Utility
{
    /// <summary>
    /// This class provides utility methods for DynamoDB.
    /// </summary>
    public static class DynamoDbUtil
    {
        /// <summary>
        /// Constructs attribute instance.
        /// </summary>
        /// <param name="attrName">Attribute name.</param>
        /// <param name="attrType">Attribute value data type.</param>
        /// <returns>Attribute instance.</returns>
        public static Provisioning.Model.TableAttribute ConstructTableAttribute(string attrName, Type attrType)
        {
            return new Provisioning.Model.TableAttribute
            {
                AttributeName = attrName,
                DataType = attrType
            };
        }

        /// <summary>
        /// Converts JSON search criteria into an instance of <see cref="SearchExpression"/>.
        /// </summary>
        /// <param name="searchCriteriaInput">Input JSON search criteria.</param>
        /// <returns>An instance of <see cref="SearchExpression"/>.</returns>
        public static SearchExpression ToSearchExpression(JToken searchCriteriaInput)
        {
            return QueryExpressionParser.Parse(searchCriteriaInput);
        }
    }
}
