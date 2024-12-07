

using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    using Common.Model;
    using Common.Constants;
    using Common.Logging;
    using Common.Provisioning;
    using Common.Query.Model;
    using Common.Provisioning.Model;
    using Common.Extensions;
    using Common.Constants.Internal;
    using Common.Exceptions;

    /// <summary>
    /// Implementation of <see cref="ICosmosDbService"/>.
    /// </summary>
    public class CosmosDbService : AbstractCosmosDbService, ICosmosDbService
    {
        private readonly Database _database;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cosmosDbConnection">An instance of <see cref="ICosmosDbConnection"/>.</param>
        /// <param name="azureCosmosDbClientFactory">An instance of <see cref="IAzureCosmosDbClientFactory"/>.</param>
        /// <param name="cosmosDbProvisioning">An instance of <see cref="ICosmosDbProvisioningConfiguration"/>.</param>
        public CosmosDbService(ICosmosDbConnection cosmosDbConnection, IAzureCosmosDbClientFactory azureCosmosDbClientFactory, ICosmosDbProvisioningConfiguration cosmosDbProvisioning) :
            base(cosmosDbConnection, azureCosmosDbClientFactory, cosmosDbProvisioning)
        {
            _database = CosmosDbClient.GetDatabase(DatabaseName);
        }

        /// <summary>
        /// Create objects in a CosmosDB table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="createInputs">List of JSON objects to create objects.</param>
        /// <returns>A <see cref="Task"/> which on completion would create objects in the database.</returns>
        public async Task<List<JObject>> CreateObjectsAsync(string tableName, List<JObject> createInputs)
        {
            var dbOperation = DbPerformanceLogFormatter.Operation.Insert;

            // Validate inputs
            ValidateInput(createInputs, dbOperation);
            ValidateInputSize(createInputs, DbConstants.BATCH_WRITE_MAX_INPUT_OBJECTS_COUNT, dbOperation);
            ContainerConfiguration tableConfig = GetTableConfig(tableName, dbOperation.ToString());
            HandleCustomFieldProcessing(tableConfig, createInputs);
            var table = _database.GetContainer(tableName);
            var createdItems = new List<JObject>(createInputs.Count);

            string ttlField = string.Empty;
            if(tableConfig.CosmosDBTimeToLiveConfig!= null)
            {
                ttlField = tableConfig.CosmosDBTimeToLiveConfig.AttributeName;
            }

            try
            {
                var tasks = createInputs.Select(async (input, index) =>
                {
                    if(!string.IsNullOrEmpty(ttlField))
                    {
                        var ttlValue = CreateTtlValue(input[ttlField]);
                        input["ttl"] = ttlValue;
                    }
                    var response = await table.CreateItemAsync(input);

                    var createdItem = JObject.FromObject(response.Resource);

                    return (index, createdItem);
                });

                var results = await Task.WhenAll(tasks);

                createdItems.AddRange(results.OrderBy(r => r.index).Select(r => r.createdItem));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while creating objects in CosmosDB.");
                throw;
            }

            return ConvertCosmosDbItems(tableConfig, createdItems);
        }

        /// <summary>
        /// Finds objects using identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="findOptions">An instance of <see cref="FindOptions"/>.</param>
        /// <returns>A <see cref="Task"/> which on completion would return objects that match given identifiers.</returns>
        public async Task<List<JObject>> FindByIdsAsync(string tableName, List<string> itemIds, string partitionKeyValue, FindOptions findOptions)
        {
            try
            {
                var dbOperation = DbPerformanceLogFormatter.Operation.Query;

                // Validate inputs
                ValidateInput(itemIds, dbOperation);
                ValidateInputSize(itemIds, DbConstants.BATCH_GET_OR_QUERY_PAGINATION_LIMIT_DEFAULT, dbOperation);
                ValidateUniqueInput(tableName, itemIds, dbOperation);
                
                ContainerConfiguration tableConfig = GetTableConfig(tableName, dbOperation.ToString());

                var table = _database.GetContainer(tableName);
                var partitionKey = new PartitionKey(partitionKeyValue);

                // Prepare the list of items to find, pairing each itemId with the partition key
                var itemsToFind = itemIds.Select(itemId => (itemId, partitionKey)).ToList();

                var response = await table.ReadManyItemsAsync<JObject>(itemsToFind);

                return ConvertCosmosDbItems(tableConfig, response.Resource?.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while finding items by IDs in CosmosDB.");
                throw;
            }
        }

        /// <summary>
        /// Permanently deletes objects by given identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="projectionFields">List of property/field names to be included in the output of delete operation. Use '*' as wildcard to return all properties.</param>
        /// <returns>A <see cref="Task"/> which on completion would delete given objects and return the deleted object.</returns>
        public async Task<List<JObject>> DeleteByIdsAsync(string tableName, string partitionKeyValue, List<string> itemIds, List<string> projectionFields)
        {
            try
            {
                var dbOperation = DbPerformanceLogFormatter.Operation.Delete;

                // Validate inputs
                ValidateInput(itemIds, dbOperation);
                ValidateInputSize(itemIds, DbConstants.BATCH_WRITE_MAX_INPUT_OBJECTS_COUNT, dbOperation);
                ContainerConfiguration tableConfig = GetTableConfig(tableName, dbOperation.ToString());

                var table = _database.GetContainer(tableName);
                var partitionKey = new PartitionKey(partitionKeyValue);

                var deleteTasks = itemIds.Select(async itemId =>
                {
                    try
                    {
                        var response = await table.DeleteItemAsync<JObject>(itemId, partitionKey);
                        return JObject.FromObject(response.Resource);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete item with ID: {itemId}, Message: {ex.Message}");
                        return null;
                    }
                });

                var deleteResults = await Task.WhenAll(deleteTasks);
                var validDeletedItems = deleteResults.Where(item => item != null).ToList();

                return ConvertCosmosDbItems(tableConfig, validDeletedItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while deleting items by IDs in CosmosDB.");
                throw;
            }
        }

        /// <summary>
        /// Permanently deletes objects by given key attribute and value across given tables.
        /// </summary>
        /// <param name="tableNames">Names of the tables where key attribute and value should be matched.</param>
        /// <param name="keyAttribute">Name of the item attribute key to match.</param>
        /// <param name="attributeValue">Value of the attribute key to match.</param>
        /// <param name="batchSize">Maximum number of objects to be deleted per iteration.</param>
        /// <returns>A <see cref="Task"/> which on completion would delete all matching items across tables defined.</returns>
        public async Task DeleteByKeyValueAsync(List<string> tableNames, string keyAttribute, object attributeValue, int batchSize = DbConstants.BULK_DELETE_MAX_OBJECTS_COUNT)
        {
            try
            {
                foreach (var tableName in tableNames)
                {
                    var table = _database.GetContainer(tableName);

                    var queryDefinition = new QueryDefinition($"SELECT c.id FROM c WHERE c.{keyAttribute} = @attributeValue")
                        .WithParameter("@attributeValue", attributeValue);

                    var queryResultSetIterator = table.GetItemQueryIterator<JObject>(queryDefinition);

                    while (queryResultSetIterator.HasMoreResults)
                    {
                        var response = await queryResultSetIterator.ReadNextAsync();
                        var itemsToDelete = response.Resource.Select(item => item["id"].ToString()).ToList();

                        // Delete items in batches
                        var deleteTasks = new List<Task>();

                        foreach (var batch in itemsToDelete.SplitList(batchSize))
                        {
                            var batchDeleteTasks = batch.Select(itemId =>
                                table.DeleteItemAsync<JObject>(itemId, new PartitionKey(attributeValue.ToString()))
                            );

                            deleteTasks.AddRange(batchDeleteTasks);
                        }

                        await Task.WhenAll(deleteTasks); 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while deleting items by key-value pair in CosmosDB.");
                throw;
            }
        }

        /// <summary>
        /// Soft deletes ('isDeleted': true) the objects by given identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="ttlAttribute">A dictionary specifying the TTL attribute for automatic removal, if applicable.</param>
        /// <returns>A <see cref="Task"/> which on completion would mark given objects as deleted.</returns>
        public async Task SoftDeleteByIdsAsync(string tableName, List<string> itemIds, string partitionKeyValue, Dictionary<string, object> ttlAttribute)
        {

            var dbOperation = DbPerformanceLogFormatter.Operation.Delete;

            ValidateInput(itemIds, dbOperation);
            var table = _database.GetContainer(tableName);

            var tasks = itemIds.Select(async itemId =>
            {
                try
                {
                    var response = await table.ReadItemAsync<JObject>(itemId, new PartitionKey(partitionKeyValue));

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var item = response.Resource;

                        item[CommonConstants.IS_DELETED] = true;

                        // Set the 'ttl' field in the item if the TTL attribute is provided
                        if (ttlAttribute != null)
                        {
                            item[CommonConstants.TTL] = JToken.FromObject(ttlAttribute[CommonConstants.PURGEDELETEDAFTERDAYS]);
                        }

                        await table.ReplaceItemAsync(item, itemId, new PartitionKey(partitionKeyValue),
                            new ItemRequestOptions { ConsistencyLevel = ConsistencyLevel.Session });
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Log a warning if the item is not found
                    Console.WriteLine($"Item with ID {itemId} not found for soft delete.");
                    throw;
                }
                catch (Exception ex)
                {
                    // Log an error if any other exception occurs
                    Console.WriteLine($"Failed to soft delete item with ID {itemId}: {ex.Message}");
                    throw;
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Undo the soft delete ('isDeleted': false) by given identifiers.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="itemIds">List of identifiers to match against the item identifier attribute.</param>
        /// <param name="partitionKeyValue">Partition key value.</param>
        /// <param name="ttlAttribute">A dictionary specifying the TTL attribute for automatic removal, if applicable.</param>
        /// <returns>A <see cref="Task"/> which on completion would mark given objects as undeleted.</returns>
        public async Task<List<JObject>> UndeleteByIdsAsync(string tableName, List<string> itemIds, string partitionKeyValue, Dictionary<string, object> ttlAttribute)
        {
            var dbOperation = DbPerformanceLogFormatter.Operation.Delete;

            ValidateInput(itemIds, dbOperation);
            ContainerConfiguration tableConfig = GetTableConfig(tableName, dbOperation.ToString());

            var table = _database.GetContainer(tableName);
            var undeletedItems = new List<JObject>();
            var partitionKey = new PartitionKey(partitionKeyValue);
            
            var tasks = itemIds.Select(async itemId =>
            {
                try
                {
                    // Read the item from the table
                    var response = await table.ReadItemAsync<JObject>(itemId, partitionKey);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var item = response.Resource;

                        // Mark the item as not deleted
                        item[CommonConstants.IS_DELETED] = false;

                        // Remove the 'ttl' field if ttlAttribute is provided
                        if (ttlAttribute != null)
                        {
                            item.Remove(CommonConstants.TTL);
                        }

                        // Replace the item in the table
                        var replaceResponse = await table.ReplaceItemAsync(item, itemId, partitionKey,
                            new ItemRequestOptions { ConsistencyLevel = ConsistencyLevel.Session });

                        // Add the undeleted item to the list
                        undeletedItems.Add(replaceResponse.Resource);
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Item with ID {itemId} not found for undelete.");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to undelete item with ID {itemId}: {ex.Message}");
                    throw;
                }
            });

            await Task.WhenAll(tasks);

            return ConvertCosmosDbItems(tableConfig, undeletedItems);
        }

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
        public async Task<List<JObject>> FindBySearchCriteriaAsync(string tableName, string indexName, string partitionKeyValue, SearchExpression searchCriteria, FindOptions findOptions, string userId)
        {
            // Validate input
            ValidateQueryInput(partitionKeyValue, searchCriteria, findOptions);

            var operation = DbPerformanceLogFormatter.Operation.Query.ToString();
            ContainerConfiguration tableConfig = GetTableConfig(tableName, operation);

            var foundItems = new List<JObject>();

            try
            {
                var partitionKeyName = tableConfig.Schema.CosmosDBKeyConfiguration.PartitionKeyPath.TrimStart('/');
                var table = _database.GetContainer(tableName);

                // Set projection fields
                string projectionFields = GetProjectionFields(findOptions?.ProjectionFields);

                // Construct query
                string queryText = BuildQueryText(tableConfig, partitionKeyName, partitionKeyValue, projectionFields, searchCriteria, findOptions);

                var queryDefinition = new QueryDefinition(queryText);
                var iterator = table.GetItemQueryIterator<JObject>(queryDefinition);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();

                    // Retrieve the ServerSideCumulativeMetrics object from the FeedResponse
                    ServerSideCumulativeMetrics metrics = response.Diagnostics.GetQueryMetrics(); // NOSONAR
                    foundItems.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine("Error occurred while querying CosmosDB NoSQL API");
                throw;
            }

            return ConvertCosmosDbItems(tableConfig, foundItems);
        }

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
        public async Task<long> CountBySearchCriteriaAsync(string tableName, string indexName, string partitionKeyValue, SearchExpression searchCriteria, FindOptions findOptions)
        {
            var operation = DbPerformanceLogFormatter.Operation.Query.ToString();
            ContainerConfiguration tableConfig = GetTableConfig(tableName, operation);

            var partitionKeyName = tableConfig.Schema.CosmosDBKeyConfiguration.PartitionKeyPath.TrimStart('/');
            var table = _database.GetContainer(tableName);

            string queryText = BuildQueryText(tableConfig, partitionKeyName, partitionKeyValue, "COUNT(1)", searchCriteria, findOptions, true);

            var queryRequestOptions = new QueryRequestOptions
            {
                MaxConcurrency = -1,
                MaxItemCount = -1,
                PartitionKey = new PartitionKey(partitionKeyValue)
            };

            long totalCount = 0;

            // Use the iterator to fetch the results
            using (var iterator = table.GetItemQueryIterator<JObject>(queryText, requestOptions: queryRequestOptions))
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();

                    // Cosmos DB returns the count in a property called "$1"
                    var countToken = response.FirstOrDefault()?["$1"];

                    if (countToken != null && countToken.Type == JTokenType.Integer)
                    {
                        totalCount += countToken.Value<long>();
                    }
                }
            }

            return totalCount;
        }


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
        public Task<List<CloneTableStatus>> CloneByKeyValueAsync(CloneInput cloneInput, string keyAttribute, string sourceAttributeValue, string targetAttributeValue, string userId)
        {
            throw new NotImplementedException();
        }

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
        public async Task<(JObject oldState, JObject newState)> UpdateByIdAsync(string tableName, string updateOperator, string itemId, string partitionKeyValue, Dictionary<string, object> updateValues, ReturnValueOption returnValueOption)
        {
            try
            {
                var operation = DbPerformanceLogFormatter.Operation.Query.ToString();
                ContainerConfiguration tableConfig = GetTableConfig(tableName, operation);
                var table = _database.GetContainer(tableName);

                var partitionKey = new PartitionKey(partitionKeyValue);
                var oldStateResponse = await table.ReadItemAsync<JObject>(itemId, partitionKey);
                var oldState = oldStateResponse.Resource;
                var updateValuesDict = HandleConversionToDBFields(tableConfig, updateValues);

                foreach (var update in updateValuesDict)
                {
                    var property = oldState.Property(update.Key);
                    if (property != null)
                    {
                        property.Value = JToken.FromObject(update.Value);
                    }
                    else
                    {
                        oldState.Add(update.Key, JToken.FromObject(update.Value));
                    }
                }

                var newStateResponse = await table.ReplaceItemAsync(oldState, itemId, partitionKey);

                if (returnValueOption == ReturnValueOption.ALL_OLD)
                {
                    return (ConvertCosmosDbItems(tableConfig, new List<JObject>() { oldStateResponse.Resource })?.FirstOrDefault(), null);
                }
                else if (returnValueOption == ReturnValueOption.ALL_NEW)
                {
                    return (null, ConvertCosmosDbItems(tableConfig, new List<JObject>() { newStateResponse.Resource })?.FirstOrDefault());
                }
                else if (returnValueOption == ReturnValueOption.ALL_OLD_NEW)
                {
                    return (ConvertCosmosDbItems(tableConfig, new List<JObject>() { oldStateResponse.Resource })?.FirstOrDefault(), ConvertCosmosDbItems(tableConfig, new List<JObject>() { newStateResponse.Resource })?.FirstOrDefault());
                }
                else
                {
                    return (null, null);
                }
            }
            catch (CosmosException ex)
            {
                // Log error if any issue occurs during the process
                Console.WriteLine($"Failed to update item with ID '{itemId}' in table '{tableName}'.");
                throw;
            }
        }

        private string GetProjectionFields(List<string> projectionFields)
        {
            return (projectionFields != null && projectionFields.Count > 0)
                ? string.Join(",", projectionFields.Select(field => field.Contains("*") ? field : $"c.{field}"))
                : "*";
        }

        private ContainerConfiguration GetTableConfig(string tableName, string operation)
        {
            if (!tableConfigMap.TryGetValue(tableName, out var tableConfig))
            {
                throw new ArgumentException($"Invalid table name(s) for {operation} operation.");
            }

            return tableConfig;
        }

        private string BuildQueryText(
        ContainerConfiguration tableConfig,
        string partitionKeyName,
        string partitionKeyValue,
        string projectionFields, // Use "COUNT(1)" for count queries
        SearchExpression searchCriteria,
        FindOptions findOptions,
        bool isCountQuery = false)
        {
            var queryText = new StringBuilder($"SELECT {projectionFields} FROM c WHERE c.{partitionKeyName} = @partitionKeyValue");

            queryText.Replace("@partitionKeyValue", $"'{partitionKeyValue}'");


            if (IsDeletedConditionRequired(searchCriteria))
            {
                queryText.Append($" AND (IS_DEFINED(c.{CommonConstants.IS_DELETED}) = false OR c.isDeleted = false)");
            }

            if (searchCriteria != null)
            {
                string searchFilter = ParseSearchExpression(tableConfig, searchCriteria);
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    queryText.Append($" AND {searchFilter}");
                }
            }

            if (!isCountQuery && findOptions?.SortField != null)
            {
                string sortDirection = findOptions.SortField.SortDirection == SortDirection.Ascending ? "ASC" : "DESC";
                queryText.Append($" ORDER BY c.{findOptions.SortField.FieldName} {sortDirection}");
            }

            if (!isCountQuery)
            {
                int skip = findOptions?.Skip ?? 0;
                int limit = findOptions?.Limit ?? DbConstants.BATCH_GET_OR_QUERY_PAGINATION_LIMIT_DEFAULT;

                queryText.Append($" OFFSET {skip} LIMIT {limit}");
            }

            return queryText.ToString();
        }


        private string ParseSearchExpression(ContainerConfiguration tableConfig, SearchExpression searchExpression)
        {
            if (searchExpression == null)
                throw new ArgumentNullException(nameof(searchExpression));

            switch (searchExpression)
            {
                case BinarySearchExpression binaryExpression:
                    return ParseBinarySearchExpression(tableConfig, binaryExpression);

                case LogicalSearchExpression logicalExpression:
                    return ParseLogicalSearchExpression(tableConfig, logicalExpression);

                default:
                    throw new InvalidOperationException("Unknown search expression type.");
            }
        }

        private string ParseBinarySearchExpression(ContainerConfiguration tableConfig, BinarySearchExpression binaryExpression)
        {
            var left = binaryExpression.Left;
            var right = binaryExpression.Right;

            string operatorSymbol = MapQueryOperator(binaryExpression.Op);

            if (left == null || right == null)
            {
                throw new ArgumentException("Binary expression must have both left and right terms.");
            }

            if (left == CommonConstants.IS_DELETED)
            {
                return string.Empty;
            }

            if(tableConfig.HandleIdFieldProcessing)
            {
                left  = HandleConversionToDBFields(tableConfig, left);
            }

            if (binaryExpression.Op == QueryOperator.In)
            {
                if (right.Value is IEnumerable<object> rightValues)
                {
                    // Format the collection for the IN operator
                    var formattedValues = string.Join(", ", rightValues.Select(FormatQueryValue));
                    return $"c.{left} IN ({formattedValues})";
                }
                else
                {
                    throw new ArgumentException("The right-hand side of an IN operator must be a collection.");
                }
            }

            return $"c.{left} {operatorSymbol} {FormatQueryValue(right.Value)}";
        }


        private string ParseLogicalSearchExpression(ContainerConfiguration tableConfig, LogicalSearchExpression logicalExpression)
        {
            var terms = logicalExpression.SearchTerms
                .Select(term => ParseSearchExpression(tableConfig, term))
                .Where(term => !string.IsNullOrWhiteSpace(term));

            string logicalOperator = MapQueryOperator(logicalExpression.Op);

            if(terms.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return $"({string.Join($" {logicalOperator} ", terms)})";
        }

        private string MapQueryOperator(QueryOperator op)
        {
            return op switch
            {
                QueryOperator.And => DBOperators.Logical.AND,
                QueryOperator.Or => DBOperators.Logical.OR,
                QueryOperator.In => DBOperators.Comparison.IN,
                QueryOperator.Eq => DBOperators.Comparison.EQ,
                QueryOperator.Gt => DBOperators.Comparison.GT,
                QueryOperator.Lt => DBOperators.Comparison.LT,
                QueryOperator.Gte => DBOperators.Comparison.GTE,
                QueryOperator.Lte => DBOperators.Comparison.LTE,
                _ => throw new ArgumentException("Unsupported query operator.")
            };
        }
        
        private bool IsDeletedConditionRequired(SearchExpression searchExpression)
        {
            if (searchExpression == null)
                return false;

            switch (searchExpression)
            {
                case BinarySearchExpression binaryExpression:
                    return binaryExpression.Left == "isDeleted";

                case LogicalSearchExpression logicalExpression:
                    return logicalExpression.SearchTerms.Any(term => IsDeletedConditionRequired(term));

                default:
                    return false;
            }
        }

        private string FormatQueryValue(object value)
        {
            if (value == null)
                return "null";

            // Handle strings and Guids by adding quotes
            if (value is string || value is Guid)
                return $"'{value}'";

            // Handle boolean values without quotes
            if (value is bool)
                return value.ToString().ToLower(); // Ensure it's "true" or "false" in lowercase

            // For other types, just return their string representation
            return value.ToString();
        }

        private void HandleCustomFieldProcessing(ContainerConfiguration tableConfig, List<JObject> createInputs)
        {
            if (!tableConfig.HandleIdFieldProcessing)
            {
                return;
            }

            Dictionary<string, string> fields = GetDbFields(tableConfig);

            foreach (var input in createInputs)
            {
                var renameMap = new Dictionary<string, JToken>();

                foreach (var key in input.Properties().Select(p => p.Name).ToList())
                {
                    if (fields.TryGetValue(key, out var dbName))
                    {
                        var value = input[key];
                        // Store it in the rename map (without modifying the input yet)
                        renameMap[dbName] = value;
                        // Remove the old key from the input
                        input.Remove(key);
                    }
                }

                // Now, apply all the renaming operations
                foreach (var kvp in renameMap)
                {
                    input[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Validates input list.
        /// </summary>
        /// <param name="inputList">Input list.</param>
        /// <param name="operation">CRUD operation.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private void ValidateInput<T>(List<T> inputList, DbPerformanceLogFormatter.Operation operation)
        {
            if (inputList.IsNullOrEmpty())
            {
                throw new ArgumentException($"Input for '{operation}' operation cannot be null or empty.");
            }
        }

        private static Dictionary<string, string> GetDbFields(ContainerConfiguration tableConfig)
        {
            // Create a dictionary from attribute Name to DBName
            return tableConfig.Schema.Attributes
                .Where(attribute => attribute.DBName != null)
                .ToDictionary(attribute => attribute.Name, attribute => attribute.DBName);
        }

        private static string HandleConversionToDBFields(ContainerConfiguration tableConfig, string queryTerm)
        {
            var fields = GetDbFields(tableConfig);
            if (fields.TryGetValue(queryTerm?.ToString(), out var dbName))
            {
                return dbName;
            }
            return null;
        }

        private static Dictionary<string, object> HandleConversionToDBFields(ContainerConfiguration tableConfig, Dictionary<string, object> queryTermDict)
        {
            if (!tableConfig.HandleIdFieldProcessing)
            {
                return queryTermDict;
            }

            var fields = GetDbFields(tableConfig);

            var convertedDict = new Dictionary<string, object>(queryTermDict.Count);

            foreach (var queryTerm in queryTermDict)
            {
                var key = fields.TryGetValue(queryTerm.Key, out var dbName) ? dbName : queryTerm.Key;
                convertedDict[key] = queryTerm.Value;
            }

            return convertedDict;
        }


        /// <summary>
        /// Validates size of input.
        /// </summary>
        /// <param name="inputList">Input list.</param>
        /// <param name="allowedSize">Allowed size of input.</param>
        /// <param name="operation">CRUD operation.</param>
        /// <exception cref="ArgumentException">Thrown when size of input exceeds allowed size.</exception>
        private void ValidateInputSize<T>(List<T> inputList, int allowedSize, DbPerformanceLogFormatter.Operation operation)
        {
            if (inputList.Count > allowedSize)
            {
                throw new ArgumentException($"Input for '{operation}' operation cannot exceed the maximum allowed size of {allowedSize}.");
            }
        }

        private long CreateTtlValue(object ttlAttributeValue)
        {
            try
            {
                long ttl = 0;

                if (ttlAttributeValue is JValue jValue)
                {
                    ttlAttributeValue = jValue.Value;
                }

                var valueType = Type.GetTypeCode(ttlAttributeValue.GetType());

                switch (valueType)
                {
                    case TypeCode.Int64:
                        long ticks = (long)ttlAttributeValue;
                        DateTime dateTimeFromTicks = new DateTime(ticks, DateTimeKind.Utc);
                        ttl = new DateTimeOffset(dateTimeFromTicks).ToUnixTimeSeconds();
                        break;

                    case TypeCode.DateTime:
                        DateTime dateTimeValue = (DateTime)ttlAttributeValue;
                        ttl = new DateTimeOffset(dateTimeValue.ToUniversalTime()).ToUnixTimeSeconds();
                        break;

                    default:
                        throw new ArgumentException($"Unsupported TTL value type: {ttlAttributeValue.GetType()}");
                }

                return ttl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not convert TTL value to CosmosDB compliant value. Reason: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validates duplicate keys in the input.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="inputList">Input list.</param>
        /// <param name="operation">CRUD operation.</param>
        /// <exception cref="DuplicateKeyException">Thrown when duplicate entries are found in the input.</exception>
        private void ValidateUniqueInput(string tableName, List<string> inputList, DbPerformanceLogFormatter.Operation operation)
        {
            var tableKeyTypesStr = string.Join(CommonConstants.FWD_SLASH_SEPARATOR, tableName);

            var duplicates = inputList.GroupBy(x => x).Where(g => g.Count() > 1).ToList();
            if (duplicates.Count > 0)
            {
                throw new DuplicateKeyException($"The input for '{operation}' operation cannot have duplicate values against '{tableKeyTypesStr}'.");
            }
        }

        /// <summary>
        /// Validates the query input.
        /// </summary>
        /// <param name="partitionKeyValue">Value of the partition key attribute.</param>
        /// <param name="searchCriteria">An instance of <see cref="SearchExpression"/>.</param>
        /// <param name="findOptions">An instance of <see cref="FindOptions"/>.</param>
        private void ValidateQueryInput(string partitionKeyValue, SearchExpression searchCriteria, FindOptions findOptions)
        {
            var dbOperation = DbPerformanceLogFormatter.Operation.Query;

            // Query needs partition key at the minimum
            ValidatePartitionKey(partitionKeyValue, dbOperation);

            // Search criteria must be supplied, otherwise its like running expensive scan operation on the table
            ValidateSearchCriteria(searchCriteria, dbOperation);

            // Validate pagination
            ValidatePaginationLimit(findOptions?.Limit.Value, DbConstants.QUERY_PAGINATION_LIMIT_MAX);
        }

        /// <summary>
        /// Validates search criteria.
        /// </summary>
        /// <param name="searchCriteria">An instance of <see cref="SearchExpression"/>.</param>
        /// <param name="operation">CRUD operation.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private void ValidateSearchCriteria(SearchExpression searchCriteria, DbPerformanceLogFormatter.Operation operation)
        {
            if (searchCriteria != null)
            {
                var logicalSearchExpr = searchCriteria as LogicalSearchExpression;
                if (logicalSearchExpr != null && logicalSearchExpr.SearchTerms.IsNullOrEmpty())
                {
                    throw new ArgumentException($"{nameof(LogicalSearchExpression)} cannot be empty for '{operation.ToString()}' operation.");
                }

                var binarySearchExpr = searchCriteria as BinarySearchExpression;
                if (binarySearchExpr != null && (binarySearchExpr.Left == null || binarySearchExpr.Right == null))
                {
                    throw new ArgumentException($"The operands in {nameof(BinarySearchExpression)} cannot be null for '{operation.ToString()}' operation.");
                }
            }
        }


        /// <summary>
        /// Validates pagination limit.
        /// </summary>
        /// <param name="inputLimit">Input pagination limit.</param>
        /// <param name="allowedLimit">Allowed pagination limit</param>
        /// <exception cref="ArgumentException">Thrown when input pagination limit exceeds allowed limit.</exception>
        private void ValidatePaginationLimit(int? inputLimit, int allowedLimit)
        {
            if (inputLimit > allowedLimit)
            {
                throw new ArgumentException($"Input pagination limit '{inputLimit}' cannot exceed the maximum allowed limit of {allowedLimit}.");
            }
        }

        /// <summary>
        /// Validates partition key input.
        /// </summary>
        /// <param name="partitionKeyValue">The partition key value.</param>
        /// <param name="operation">CRUD operation.</param>
        private void ValidatePartitionKey(object partitionKeyValue, DbPerformanceLogFormatter.Operation operation)
        {
            if (partitionKeyValue == null)
            {
                throw new ArgumentException($"Null value input for partition key is not allowed for '{operation}' operation.");
            }
        }

    }
}
