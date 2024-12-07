

using Newtonsoft.Json.Linq;
using NoSqlDataAccess.Common.Query.Model;
using NoSqlDataAccess.Common.Model;
using NoSqlDataAccess.Common.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoSqlDataAccess.Common
{
    /// <summary>
    /// Common interface for NoSQL database admin operations.
    /// </summary>
    public interface INoSqlDbService
    {
        /// <summary>
        /// Create objects.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="createInputs">List of JSON objects to create objects.</param>
        /// <returns>A <see cref="Task"/> which on completion would create objects in the database.</returns>
        Task<List<JObject>> CreateObjectsAsync(string tableName, List<JObject> createInputs);

        /// <summary>
        /// Finds objects using identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="findOptions">An instance of <see cref="FindOptions"/>.</param>
        /// <returns>A <see cref="Task"/> which on completion would return objects that match given identifiers.</returns>
        Task<List<JObject>> FindByIdsAsync(string tableName, List<string> itemIds, string partitionKeyValue, FindOptions findOptions);

        /// <summary>
        /// Finds objects using search criteria.
        /// This API requires the primary key indentifiers to minimize the search data size.
        /// Optionally the range key value can be supplied to further reduce the search data size.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="indexName">Name of the global secondary index to query (optional).</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="searchCriteria">An instance of <see cref="SearchExpression"/> that represents the search criteria.</param>
        /// <param name="findOptions">An instance of <see cref="FindOptions"/>.</param>
        /// <param name="userId">Identifier of the user performing the query.</param>
        /// <returns>A <see cref="Task"/> which on completion would return objects that match the query criteria.</returns>
        Task<List<JObject>> FindBySearchCriteriaAsync(string tableName, string indexName, string partitionKeyValue, SearchExpression searchCriteria, FindOptions findOptions, string userId);

        /// <summary>
        /// Count objects using search criteria.
        /// This API requires the primary key indentifiers to minimize the search data size.
        /// Optionally the range key value can be supplied to further reduce the search data size.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="indexName">Name of the global secondary index to query (optional).</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="searchCriteria">An instance of <see cref="SearchExpression"/> that represents the search criteria.</param>
        /// <param name="findOptions">An instance of <see cref="FindOptions"/>.</param>
        /// <returns>A <see cref="Task"/> which on completion would return objects that match the query criteria.</returns>
        Task<long> CountBySearchCriteriaAsync(string tableName, string indexName, string partitionKeyValue, SearchExpression searchCriteria, FindOptions findOptions);

        /// <summary>
        /// Permanently deletes objects by given identifiers.
        /// This API assumes that the schema of the given table defines an index where:
        ///     - PrimaryKey/PartitionKey: <paramref name="partitionKeyValue"/> - is an identifier of a table in which given item identifiers are referenced.
        ///     - SortKey/RangeKey: <paramref name="itemIds"/> - is an identifier of the item to be deleted.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="projectionFields">List of property/field names to be included in the output of delete operation. Use '*' as wildcard to return all properties.</param>
        /// <returns>A <see cref="Task"/> which on completion would delete given objects and return the deleted object.</returns>
        Task<List<JObject>> DeleteByIdsAsync(string tableName, string partitionKeyValue, List<string> itemIds, List<string> projectionFields);

        /// <summary>
        /// Permanently deletes objects by given key attribute and value across given tables.
        /// Conditions:
        ///     1. The key attribute must be configured as the primary/partition key attribute on at least one of the following:
        ///         - table
        ///         - one of the global secondary indexes.
        ///                         
        /// Delete operation will be performed iteratively, and will continue until the objects with matching key and value are exausted from the table.
        /// </summary>
        /// <param name="tableNames">Names of the tables where key attribute and value should be matched.</param>
        /// <param name="keyAttribute">Name of the item attribute key to match.</param>
        /// <param name="attributeValue">Value of the attribute key to match.</param>
        /// <param name="batchSize">Maximum number of objects to be deleted per iteration.</param>
        /// <returns>A <see cref="Task"/> which on completion would delete all matching items across tables defined.</returns>
        Task DeleteByKeyValueAsync(List<string> tableNames, string keyAttribute, object attributeValue, int batchSize = DbConstants.BULK_DELETE_MAX_OBJECTS_COUNT);

        /// <summary>
        /// Clones objects using given key attribute and its value across given tables.
        /// When objects are cloned from given source tables, the source attribute value will be replaced with target attribute value.
        /// </summary>
        /// <param name="cloneInput">An instance of <see cref="CloneInput"/>. For initial clone this can be empty or null. For subsequent clone invocations the previous return value from this method should be used.</param>
        /// <param name="keyAttribute">The attribute key using which the items from given tables will be cloned.</param>
        /// <param name="sourceAttributeValue">Source value of the attribute key.</param>
        /// <param name="targetAttributeValue">Target value of the attribute key.</param>
        /// <param name="userId">User identifier.</param>
        /// <returns>A <see cref="Task"/> which on completion will clone objects using given criteria and size, and return an instance of <see cref="List{CloneTableStatus}"/>.</returns>
        Task<List<CloneTableStatus>> CloneByKeyValueAsync(CloneInput cloneInput, string keyAttribute, string sourceAttributeValue, string targetAttributeValue, string userId);

        /// <summary>
        /// Soft deletes ('isDeleted': true) the objects by given identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="ttlAttribute">A dictionary specifying the TTL attribute for automatic removal, if applicable.</param>
        /// <returns>A <see cref="Task"/> which on completion would mark given objects as deleted.</returns>
        Task SoftDeleteByIdsAsync(string tableName, List<string> itemIds, string partitionKeyValue, Dictionary<string, object> ttlAttribute);

        /// <summary>
        /// Undo the soft delete ('isDeleted': false) by given identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="ttlAttribute">A dictionary specifying the TTL attribute for automatic removal, if applicable.</param>
        /// <returns>A <see cref="Task"/> which on completion would mark given objects as undeleted.</returns>
        Task<List<JObject>> UndeleteByIdsAsync(string tableName, List<string> itemIds, string partitionKeyValue, Dictionary<string, object> ttlAttribute);

        /// <summary>
        /// Updates object using given identifier.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="updateOperator">The operator to use for update operation.</param>
        /// <param name="itemId">Identifier to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="updateValues">Key-value pairs of attribute name and values to be updated to matching objects.</param>
        /// <param name="returnValueOption">Specifies whether and how the state of update attributes should be returned.</param>
        /// <returns>A <see cref="Task"/> which on completion would update objects and return the state of object atributes before and after updated operation.</returns>
        Task<(JObject oldState, JObject newState)> UpdateByIdAsync(string tableName, string updateOperator, string itemId, string partitionKeyValue, Dictionary<string, object> updateValues, ReturnValueOption returnValueOption);
    }
}
