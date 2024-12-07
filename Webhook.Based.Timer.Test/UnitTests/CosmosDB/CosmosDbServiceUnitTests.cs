

using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NoSqlDataAccess.CosmosDB.Test.UnitTests.CosmosDB
{
    using NoSqlDataAccess.Azure.CosmosDB.DBService;
    using NoSqlDataAccess.Common.Constants.Internal;
    using NoSqlDataAccess.Common.Exceptions;
    using NoSqlDataAccess.Common.Model;
    using NoSqlDataAccess.Common.Provisioning;
    using NoSqlDataAccess.Common.Provisioning.Model;
    using NoSqlDataAccess.Common.Query;
    using NoSqlDataAccess.Common.Query.Model;
    using System.Data;

    [TestFixture]
    [Category("TestSuite.Unit")]
    internal class CosmosDbServiceUnitTests
    {
        private Mock<ICosmosDbConnection> _mockCosmosDbConnection;
        private Mock<IAzureCosmosDbClientFactory> _mockCosmosDbClientFactory;
        private Mock<ICosmosDbProvisioningConfiguration> _mockProvisioningConfiguration;
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Database> _mockDatabase;
        private Mock<Container> _mockContainer;
        private Mock<ICosmosDbProvisioningConfiguration> _mockProvisioning;
        private CosmosDbService _cosmosDbService;

        [SetUp]
        public void SetUp()
        {
            _mockCosmosDbConnection = new Mock<ICosmosDbConnection>();
            _mockCosmosDbClientFactory = new Mock<IAzureCosmosDbClientFactory>();
            _mockProvisioningConfiguration = new Mock<ICosmosDbProvisioningConfiguration>();
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockDatabase = new Mock<Database>();
            _mockContainer = new Mock<Container>();
            _mockProvisioning = new Mock<ICosmosDbProvisioningConfiguration>();

            _mockCosmosDbConnection.Setup(c => c.EndpointUri).Returns("https://localhost:8081");
            _mockCosmosDbConnection.Setup(c => c.DatabaseName).Returns("TestDatabase");
            _mockCosmosDbConnection.Setup(c => c.PrimaryKey).Returns("PrimaryKey");

            _mockCosmosDbClientFactory.Setup(f => f.GetCosmosDbClient(It.IsAny<string>(), It.IsAny<string>()))
                                       .ReturnsAsync(_mockCosmosClient.Object);

            _mockCosmosClient.Setup(c => c.GetDatabase("TestDatabase"))
                             .Returns(_mockDatabase.Object);
            _mockDatabase.Setup(d => d.GetContainer("TestTable"))
                         .Returns(_mockContainer.Object);

            _cosmosDbService = new CosmosDbService(
                _mockCosmosDbConnection.Object,
                _mockCosmosDbClientFactory.Object,
                _mockProvisioningConfiguration.Object
            );
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_CreateObjectsAsync_ValidInputs_CreatesItems()
        {
            // Arrange
            var createInputs = new List<JObject>
            {
                new JObject { { "id", "1" }, { "name", "Item1" } , { "ttl", 10} },
                new JObject { { "id", "2" }, { "name", "Item2" }, { "ttl", new DateTime() } }
            };

            var tableName = "TestTable";
            var containerConfig = new ContainerConfiguration
             {
                 CosmosDBTimeToLiveConfig = new CosmosDBTimeToLiveConfig { AttributeName = "ttl" }
             };
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" }, Attributes = new List<ContainerAttribute>{ new ContainerAttribute { DBName = "TestDbName" , Name = "id" } } },HandleIdFieldProcessing = true , CosmosDBTimeToLiveConfig = new CosmosDBTimeToLiveConfig { AttributeName = "ttl" , TtlInSeconds = 10 , Enabled = true }} }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);

            

            _mockProvisioningConfiguration.Setup(p => p.ContainerConfigurations)
                                          .Returns(new List<ContainerConfiguration> { containerConfig });

            var mockItemResponse = new Mock<ItemResponse<JObject>>();
            mockItemResponse.Setup(m => m.Resource).Returns(new JObject { { "id", "1" }, { "name", "Item1" } });

            _mockContainer.Setup(c => c.CreateItemAsync(It.IsAny<JObject>(), null, null, default))
                          .ReturnsAsync(mockItemResponse.Object);

            // Act
            var result = await _cosmosDbService.CreateObjectsAsync(tableName, createInputs);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void CosmosDbServiceUnitTests_CreateObjectsAsync_NullInputs_ThrowsException()
        {
            // Arrange
            List<JObject> createInputs = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDbService.CreateObjectsAsync("TestTable", createInputs));
        }

        [Test]
        public void CosmosDbServiceUnitTests_CreateObjectsAsync_EmptyInputs_ThrowsException()
        {
            // Arrange
            var createInputs = new List<JObject>();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDbService.CreateObjectsAsync("TestTable", createInputs));
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_CreateObjectsAsync_CreateItemThrowsException_LogsError()
        {
            // Arrange
            var createInputs = new List<JObject>
            {
                new JObject { { "id", "1" }, { "name", "Item1" } }
            };

            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            _mockContainer.Setup(c => c.CreateItemAsync(It.IsAny<JObject>(), null, null, default))
                          .ThrowsAsync(new Exception("CosmosDB error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _cosmosDbService.CreateObjectsAsync(tableName, createInputs));
            Assert.AreEqual("CosmosDB error", ex.Message);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_FindByIdsAsync_ValidInputs_ReturnsItems()
        {
            // Arrange
            var itemIds = new List<string> { "1", "2" };
            var partitionKeyValue = "somePartitionKey";
            var findOptions = new FindOptions();

            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);

            var mockResponse = new Mock<FeedResponse<JObject>>();
            mockResponse.Setup(r => r.Resource).Returns(new List<JObject>
            {
                new JObject { { "id", "1" }, { "name", "Item1" } },
                new JObject { { "id", "2" }, { "name", "Item2" } }
            });
    
        _mockContainer.Setup(c => c.ReadManyItemsAsync<JObject>(It.IsAny<IReadOnlyList<(string , PartitionKey )>>(),It.IsAny<ReadManyRequestOptions>(),It.IsAny<CancellationToken>()))
                          .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await _cosmosDbService.FindByIdsAsync(tableName, itemIds, partitionKeyValue, findOptions);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void CosmosDbServiceUnitTests_FindByIdsAsync_NullItemIds_ThrowsArgumentException()
        {
            // Arrange
            List<string> itemIds = null;
            var partitionKeyValue = "somePartitionKey";
            var findOptions = new FindOptions();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDbService.FindByIdsAsync("TestTable", itemIds, partitionKeyValue, findOptions));

            Assert.AreEqual("Input for 'Query' operation cannot be null or empty.", ex.Message);
        }

        [Test]
        public void CosmosDbServiceUnitTests_FindByIdsAsync_EmptyItemIds_ThrowsArgumentException()
        {
            // Arrange
            var itemIds = new List<string>();
            var partitionKeyValue = "somePartitionKey";
            var findOptions = new FindOptions();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDbService.FindByIdsAsync("TestTable", itemIds, partitionKeyValue, findOptions));

            Assert.AreEqual("Input for 'Query' operation cannot be null or empty.", ex.Message);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_FindByIdsAsync_DuplicateItemIds_ThrowsDuplicateKeyException()
        {
            // Arrange
            var itemIds = new List<string> { "1", "1" }; // Duplicate IDs
            var partitionKeyValue = "somePartitionKey";
            var findOptions = new FindOptions();

            // Act & Assert
            var ex = Assert.ThrowsAsync<DuplicateKeyException>(async () =>
                await _cosmosDbService.FindByIdsAsync("TestTable", itemIds, partitionKeyValue, findOptions));

            Assert.AreEqual("The input for 'Query' operation cannot have duplicate values against 'TestTable'.", ex.Message);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_FindByIdsAsync_ReadManyItemsThrowsException_LogsError()
        {
            // Arrange
            var itemIds = new List<string> { "1", "2" };
            var partitionKeyValue = "somePartitionKey";
            var findOptions = new FindOptions();

            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);

            _mockContainer.Setup(c => c.ReadManyItemsAsync<JObject>(It.IsAny<IReadOnlyList<(string, PartitionKey)>>(), It.IsAny<ReadManyRequestOptions>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("CosmosDB error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _cosmosDbService.FindByIdsAsync("TestTable", itemIds, partitionKeyValue, findOptions));

            Assert.AreEqual("CosmosDB error", ex.Message);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_DeleteByIdsAsync_ValidInputs_DeletesItems()
        {
            // Arrange
            var itemIds = new List<string> { "1" };
            var partitionKeyValue = "somePartitionKey";
            var projectionFields = new List<string>();
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var mockResponse1 = new Mock<ItemResponse<JObject>>();
            mockResponse1.Setup(r => r.Resource).Returns(new JObject { { "id", "1" }, { "name", "Item1" } });

            _mockContainer.Setup(c => c.DeleteItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(mockResponse1.Object);
            
            // Act
            var result = await _cosmosDbService.DeleteByIdsAsync(tableName, partitionKeyValue, itemIds, projectionFields);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1", result[0]["id"].ToString());
        }

        [Test]
        public void CosmosDbServiceUnitTests_DeleteByIdsAsync_NullItemIds_ThrowsArgumentException()
        {
            // Arrange
            List<string> itemIds = null;
            var partitionKeyValue = "somePartitionKey";
            var projectionFields = new List<string>();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDbService.DeleteByIdsAsync("TestTable", partitionKeyValue, itemIds, projectionFields));

            Assert.AreEqual("Input for 'Delete' operation cannot be null or empty.", ex.Message);
        }

        [Test]
        public void CosmosDbServiceUnitTests_DeleteByIdsAsync_EmptyItemIds_ThrowsArgumentException()
        {
            // Arrange
            var itemIds = new List<string>();
            var partitionKeyValue = "somePartitionKey";
            var projectionFields = new List<string>();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDbService.DeleteByIdsAsync("TestTable", partitionKeyValue, itemIds, projectionFields));

            Assert.AreEqual("Input for 'Delete' operation cannot be null or empty.", ex.Message);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_DeleteByIdsAsync_NonEmptyItemIds_ThrowsException()
        {
            // Arrange
            var itemIds = new List<string> { "1" };
            var partitionKeyValue = "somePartitionKey";
            var projectionFields = new List<string>();
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var mockResponse1 = new Mock<ItemResponse<JObject>>();
            mockResponse1.Setup(r => r.Resource).Returns(new JObject { { "id", "1" }, { "name", "Item1" } });

            _mockContainer.Setup(c => c.DeleteItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                          .Throws(new Exception("Failed to delete"));

            // Act
            var result = await _cosmosDbService.DeleteByIdsAsync(tableName, partitionKeyValue, itemIds, projectionFields);

            // Assert
            Assert.IsNotNull(result);       
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_DeleteByKeyValueAsync_DeletesItemsSuccessfully()
        {
            // Arrange
            var tableNames = new List<string> { "TestTable" };
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var keyAttribute = "TestKey";
            var attributeValue = "TestValue";
            var itemsToDelete = new List<JObject>
            {
                JObject.FromObject(new { id = "1" }),
                JObject.FromObject(new { id = "2" })
            };

            var queryDefinition = new QueryDefinition($"SELECT c.id FROM c WHERE c.{keyAttribute} = @attributeValue")
                .WithParameter("@attributeValue", attributeValue);
            var queryIterator = new Mock<FeedIterator<JObject>>();

            queryIterator.SetupSequence(x => x.HasMoreResults)
                .Returns(true)
                .Returns(false);
            var mockFeedResponse = new Mock<FeedResponse<JObject>>();

            mockFeedResponse.Setup(r => r.GetEnumerator()).Returns(itemsToDelete.GetEnumerator());
            queryIterator.Setup(x => x.ReadNextAsync(default))
                .ReturnsAsync(mockFeedResponse.Object);
            mockFeedResponse.Setup(s => s.Resource).Returns(mockFeedResponse.Object);
            
            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(queryIterator.Object);

            // Act
            await _cosmosDbService.DeleteByKeyValueAsync(tableNames, keyAttribute, attributeValue);

            // Assert
            _mockContainer.Verify(x => x.DeleteItemAsync<JObject>("1", It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockContainer.Verify(x => x.DeleteItemAsync<JObject>("2", It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CosmosDbServiceUnitTests_DeleteByKeyValueAsync_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var tableNames = new List<string> { "TestTable" };
            var keyAttribute = "TestKey";
            var attributeValue = "TestValue";

            var queryIterator = new Mock<FeedIterator<JObject>>();

            queryIterator.Setup(x => x.HasMoreResults).Returns(true);
            queryIterator.Setup(x => x.ReadNextAsync(default))
                .ThrowsAsync(new CosmosException("Error occurred", System.Net.HttpStatusCode.BadRequest, 0, "", 0));

            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(queryIterator.Object);

            // Act & Assert
            Assert.ThrowsAsync<CosmosException>(() => _cosmosDbService.DeleteByKeyValueAsync(tableNames, keyAttribute, attributeValue));
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_FindBySearchCriteriaAsync_ReturnsFoundItems()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);

            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions { ProjectionFields = new List<string> { "field1", "field2" } };
            var userId = "TestUser";

            var tableConfig = new ContainerConfiguration
            {
                Schema = new ContainerSchema
                {
                    CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration
                    {
                        PartitionKeyPath = "/TestPartitionKey"
                    }
                }
            };

            _mockProvisioning.Setup(x => x.ContainerConfigurations)
                .Returns(new List<ContainerConfiguration> { tableConfig });

            var responseItems = new List<JObject>
            {
                JObject.FromObject(new { id = "1", field1 = "value1", field2 = "value2" }),
                JObject.FromObject(new { id = "2", field1 = "value3", field2 = "value4" })
            };

            var ServerSideCumulativeMetrics = new Mock<ServerSideCumulativeMetrics>();
            var mockFeedResponse = new Mock<FeedResponse<JObject>>();
            mockFeedResponse.Setup(s => s.Diagnostics.GetQueryMetrics())
                .Returns(ServerSideCumulativeMetrics.Object);
            mockFeedResponse.Setup(r => r.GetEnumerator()).Returns(responseItems.GetEnumerator());
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.TestPartitionKey = @pkValue")
                .WithParameter("@pkValue", partitionKeyValue);
            var iterator = new Mock<FeedIterator<JObject>>();

            iterator.SetupSequence(x => x.HasMoreResults)
                .Returns(true)
                .Returns(false);

            iterator.Setup(s => s.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFeedResponse.Object);

            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iterator.Object);

            // Act
            var result = await _cosmosDbService.FindBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions, userId);

            // Assert
            Assert.AreEqual(2, result.Count);
            _mockContainer.Verify(x => x.GetItemQueryIterator<JObject>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()), Times.Once);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_FindBySearchCriteriaAsync_ReturnsArgumentException_for_ValidatePartitionKey()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);

            var indexName = "TestIndex";
            string partitionKeyValue = null;
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions { ProjectionFields = new List<string> { "field1", "field2" } };
            var userId = "TestUser";

            var tableConfig = new ContainerConfiguration
            {
                Schema = new ContainerSchema
                {
                    CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration
                    {
                        PartitionKeyPath = "/TestPartitionKey"
                    }
                }
            };

            _mockProvisioning.Setup(x => x.ContainerConfigurations)
                .Returns(new List<ContainerConfiguration> { tableConfig });

            // Act
            Assert.ThrowsAsync<ArgumentException>(() => _cosmosDbService.FindBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions, userId));
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_FindBySearchCriteriaAsync_ReturnsArgumentException_for_ValidatePaginationLimit()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);

            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions { ProjectionFields = new List<string> { "field1", "field2" }, Limit = 501 };
            var userId = "TestUser";

            var tableConfig = new ContainerConfiguration
            {
                Schema = new ContainerSchema
                {
                    CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration
                    {
                        PartitionKeyPath = "/TestPartitionKey"
                    }
                }
            };

            _mockProvisioning.Setup(x => x.ContainerConfigurations)
                .Returns(new List<ContainerConfiguration> { tableConfig });

            // Act
            Assert.ThrowsAsync<ArgumentException>(() => _cosmosDbService.FindBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions, userId));
        }

        [Test]
        public void CosmosDbServiceUnitTests_FindBySearchCriteriaAsync_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions { ProjectionFields = new List<string> { "field1", "field2" } };
            var userId = "TestUser";

            var tableConfig = new ContainerConfiguration
            {
                Schema = new ContainerSchema
                {
                    CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration
                    {
                        PartitionKeyPath = "/TestPartitionKey"
                    }
                }
            };

            _mockProvisioning.Setup(x => x.ContainerConfigurations)
                .Returns(new List<ContainerConfiguration> { tableConfig });

            var iterator = new Mock<FeedIterator<JObject>>();

            iterator.Setup(x => x.HasMoreResults).Returns(true);
            iterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("Error occurred", System.Net.HttpStatusCode.BadRequest, 0, "", 0));

            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iterator.Object);

            // Act & Assert
            Assert.ThrowsAsync<CosmosException>(() => _cosmosDbService.FindBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions, userId));
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_SoftDeleteByIdsAsync_SoftDeletesItemsSuccessfully()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemIds = new List<string> { "item1", "item2" };
            var partitionKeyValue = "TestPartitionKey";

            // TTL attribute (e.g., set TTL to 5 days from now)
            var ttlValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(5)).ToUnixTimeSeconds();
            var ttlAttribute = new Dictionary<string, object>
            {
                {  CommonConstants.PURGEDELETEDAFTERDAYS, ttlValue }
            };

            var mockItem1 = new JObject { ["id"] = "item1", [CommonConstants.IS_DELETED] = false };
            

            var response1 = new Mock<ItemResponse<JObject>>();
            response1.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response1.Setup(s => s.Resource).Returns(mockItem1);
          
            _mockContainer.Setup(x => x.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response1.Object);
            _mockContainer.Setup(x => x.ReplaceItemAsync(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response1.Object);

            // Act
            await _cosmosDbService.SoftDeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute);

            // Assert
            _mockContainer.Verify(x => x.ReadItemAsync<JObject>("item1", new PartitionKey(partitionKeyValue), null, default), Times.Once);
            _mockContainer.Verify(x => x.ReadItemAsync<JObject>("item2", new PartitionKey(partitionKeyValue), null, default), Times.Once);
            _mockContainer.Verify(x => x.ReplaceItemAsync(mockItem1, "item1", new PartitionKey(partitionKeyValue), It.IsAny<ItemRequestOptions>(), default), Times.Once);
            _mockContainer.Verify(x => x.ReplaceItemAsync(It.Is<JObject>(j => (bool)j[CommonConstants.IS_DELETED] == true &&
                                                               j["ttl"].Value<long>() == ttlValue),
                                                               "item2", new PartitionKey(partitionKeyValue), It.IsAny<ItemRequestOptions>(), default), Times.Once);
        }

        [Test]
        public void CosmosDbServiceUnitTests_SoftDeleteByIdsAsync_ThrowsArgumentException_WhenInputIsEmpty()
        {
            // Arrange
            var tableName = "TestTable";
            var itemIds = new List<string>(); // Empty list
            var partitionKeyValue = "TestPartitionKey";

            // TTL attribute (e.g., set TTL to 5 days from now)
            var ttlValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(5)).ToUnixTimeSeconds();
            var ttlAttribute = new Dictionary<string, object>
            {
                {  CommonConstants.PURGEDELETEDAFTERDAYS, ttlValue }
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _cosmosDbService.SoftDeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute));
            Assert.That(ex.Message, Is.EqualTo("Input for 'Delete' operation cannot be null or empty."));
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_SoftDeleteByIdsAsync_LogsWarning_WhenItemNotFound()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemIds = new List<string> { "item1" };
            var partitionKeyValue = "TestPartitionKey";

            // TTL attribute (e.g., set TTL to 5 days from now)
            var ttlValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(5)).ToUnixTimeSeconds();
            var ttlAttribute = new Dictionary<string, object>
            {
                {  CommonConstants.PURGEDELETEDAFTERDAYS, ttlValue }
            };

            var mockItem = new JObject { ["id"] = "item1", [CommonConstants.IS_DELETED] = false };

            var response1 = new Mock<ItemResponse<JObject>>();
            response1.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response1.Setup(s => s.Resource).Returns(mockItem);
            _mockContainer.Setup(x => x.ReadItemAsync<JObject>("item1", new PartitionKey(partitionKeyValue), null, default))
                .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));

            // Act
            var ex = Assert.ThrowsAsync<CosmosException>(() => _cosmosDbService.SoftDeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute));
        }

        [Test]
        public void CosmosDbServiceUnitTests_SoftDeleteByIdsAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var tableName = "TestTable";
            var itemIds = new List<string> { "item1" };
            var partitionKeyValue = "TestPartitionKey";

            // TTL attribute (e.g., set TTL to 5 days from now)
            var ttlValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(5)).ToUnixTimeSeconds();
            var ttlAttribute = new Dictionary<string, object>
            {
                { CommonConstants.PURGEDELETEDAFTERDAYS, ttlValue }
            };

            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            
            _mockContainer.Setup(x => x.ReadItemAsync<JObject>("item1", new PartitionKey(partitionKeyValue), null, default))
                .ThrowsAsync(new Exception("Some error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _cosmosDbService.SoftDeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute));
            Assert.That(ex.Message, Is.EqualTo("Some error"));

        }

        [Test]
        public async Task CosmosDbServiceUnitTests_UndeleteByIdsAsync_ItemFound_UndeletesSuccessfully()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemIds = new List<string> { "item1" };
            var partitionKeyValue = "TestPartitionKey";

            var mockItem = new JObject { ["id"] = "item1", [CommonConstants.IS_DELETED] = true };

            // TTL attribute (e.g., set TTL to 5 days from now)
            var ttlValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(5)).ToUnixTimeSeconds();
            var ttlAttribute = new Dictionary<string, object>
            {
                { CommonConstants.PURGEDELETEDAFTERDAYS, null }
            };

            var response = new Mock<ItemResponse<JObject>>();
            response.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response.Setup(s => s.Resource).Returns(mockItem);

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);
            _mockContainer.Setup(x => x.ReplaceItemAsync(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response.Object);

            // Act
            var result = await _cosmosDbService.UndeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsFalse((bool)result[0][CommonConstants.IS_DELETED]); // Check if the item is now undeleted
            // After calling UndeleteByIdsAsync, the 'ttl' attribute should be removed from the item,
            Assert.That(result[0].ContainsKey("ttl"), Is.False);

            _mockContainer.Verify(x => x.ReadItemAsync<JObject>("item1", new PartitionKey(partitionKeyValue), null, default), Times.Once);
            _mockContainer.Verify(x => x.ReplaceItemAsync(mockItem, "item1", new PartitionKey(partitionKeyValue), It.IsAny<ItemRequestOptions>(), default), Times.Once);
        }

        [Test]
        public void CosmosDbServiceUnitTests_UndeleteByIdsAsync_ItemNotFound_LogsWarning()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            var ttlAttribute = new Dictionary<string, object>
            {
                { CommonConstants.PURGEDELETEDAFTERDAYS, null }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemIds = new List<string> { "item2" };
            var partitionKeyValue = "TestPartitionKey";

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>("item2", new PartitionKey(partitionKeyValue), null, default))
                .ThrowsAsync(new CosmosException("Not found", HttpStatusCode.NotFound, 0, "item2", 0));

            // Act & Assert
            Assert.ThrowsAsync<CosmosException>(async () => await _cosmosDbService.UndeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute));
        }

        [Test]
        public void CosmosDbServiceUnitTests_UndeleteByIdsAsync_ItemNotFound_LogsException()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            var ttlAttribute = new Dictionary<string, object>
            {
                {  CommonConstants.PURGEDELETEDAFTERDAYS, null }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemIds = new List<string> { "item2" };
            var partitionKeyValue = "TestPartitionKey";

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>("item2", new PartitionKey(partitionKeyValue), null, default))
                .ThrowsAsync(new Exception("Failed to Undelete"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _cosmosDbService.UndeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute));
        }

        [Test]
        public void CosmosDbServiceUnitTests_UndeleteByIdsAsync_EmptyItemIds_ThrowsArgumentException()
        {
            // Arrange
            var tableName = "TestTable";
            var itemIds = new List<string>(); // Empty list
            var partitionKeyValue = "TestPartitionKey";

            // TTL attribute (e.g., set TTL to 5 days from now)
            var ttlValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(5)).ToUnixTimeSeconds();
            var ttlAttribute = new Dictionary<string, object>
            {
                { CommonConstants.PURGEDELETEDAFTERDAYS, ttlValue }
            };
           
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _cosmosDbService.UndeleteByIdsAsync(tableName, itemIds, partitionKeyValue, ttlAttribute));
            Assert.That(ex.Message, Is.EqualTo("Input for 'Delete' operation cannot be null or empty."));
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_CountBySearchCriteriaAsync_ReturnsCorrectCount()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions(); 

            var mockResult = new JObject { ["$1"] = 5 }; 
            var mockResponse = new List<JObject> { mockResult };
            var iteratorMock = new Mock<FeedIterator<JObject>>();
            
            var mockFeedResponse = new Mock<FeedResponse<JObject>>();
            mockFeedResponse.Setup(s => s.GetEnumerator()).Returns(mockResponse.GetEnumerator());
            // Setup the query iterator to return the mocked response
            iteratorMock.SetupSequence(x => x.HasMoreResults)
                .Returns(true) // First call returns true
                .Returns(false); // Second call returns false

            iteratorMock.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFeedResponse.Object);
            
            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iteratorMock.Object);

            // Act
            var result = await _cosmosDbService.CountBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions);

            // Assert
            Assert.AreEqual(5, result);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_CountBySearchCriteriaAsync_With_isDeleted_ReturnsCorrectCount()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } }, HandleIdFieldProcessing = true } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'isDeleted',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions();

            var mockResult = new JObject { ["$1"] = 5 };
            var mockResponse = new List<JObject> { mockResult };
            var iteratorMock = new Mock<FeedIterator<JObject>>();

            var mockFeedResponse = new Mock<FeedResponse<JObject>>();
            mockFeedResponse.Setup(s => s.GetEnumerator()).Returns(mockResponse.GetEnumerator());
            // Setup the query iterator to return the mocked response
            iteratorMock.SetupSequence(x => x.HasMoreResults)
                .Returns(true) // First call returns true
                .Returns(false); // Second call returns false

            iteratorMock.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFeedResponse.Object);

            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iteratorMock.Object);

            // Act
            var result = await _cosmosDbService.CountBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions);

            // Assert
            Assert.AreEqual(5, result);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_CountBySearchCriteriaAsync_With_HandleIdFieldProcessing_ReturnsCorrectCount()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" }, Attributes = new List<ContainerAttribute>{ new ContainerAttribute { DBName = "TestDatabase" , Name = "name" } } }, HandleIdFieldProcessing = true } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions();

            var mockResult = new JObject { ["$1"] = 5 };
            var mockResponse = new List<JObject> { mockResult };
            var iteratorMock = new Mock<FeedIterator<JObject>>();

            var mockFeedResponse = new Mock<FeedResponse<JObject>>();
            mockFeedResponse.Setup(s => s.GetEnumerator()).Returns(mockResponse.GetEnumerator());
            // Setup the query iterator to return the mocked response
            iteratorMock.SetupSequence(x => x.HasMoreResults)
                .Returns(true) // First call returns true
                .Returns(false); // Second call returns false

            iteratorMock.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFeedResponse.Object);

            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iteratorMock.Object);

            // Act
            var result = await _cosmosDbService.CountBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions);

            // Assert
            Assert.AreEqual(5, result);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_CountBySearchCriteriaAsync_ReturnsZeroWhenNoResults()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var indexName = "TestIndex";
            var partitionKeyValue = "TestPartitionKey";
            var rawJson = @"{
                'op': 'And',
                'searchTerms': [
                    {
                        'op': 'Eq',
                        'left': 'name',
                        'right': 'NonExistent_Dataset'
                    }
                ]
            }";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var findOptions = new FindOptions(); 

            var iteratorMock = new Mock<FeedIterator<JObject>>();

            // Setup the query iterator to return no results
            iteratorMock.Setup(x => x.HasMoreResults).Returns(false);

            _mockContainer.Setup(x => x.GetItemQueryIterator<JObject>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(iteratorMock.Object);

            // Act
            var result = await _cosmosDbService.CountBySearchCriteriaAsync(tableName, indexName, partitionKeyValue, searchCriteria, findOptions);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_UpdateByIdAsync_UpdatesItemAndReturnsNewState()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemId = "testItemId";
            var partitionKeyValue = "testPartitionKey";
            var updateValues = new Dictionary<string, object> { { "Property1", "UpdatedValue" } };
            var returnValueOption = ReturnValueOption.ALL_NEW;

            var oldStateJObject = new JObject { ["Property1"] = "OldValue" };
            var newStateJObject = new JObject { ["Property1"] = "UpdatedValue" };

            var response1 = new Mock<ItemResponse<JObject>>();
            response1.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response1.Setup(s => s.Resource).Returns(oldStateJObject);

            var response2 = new Mock<ItemResponse<JObject>>();
            response2.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response2.Setup(s => s.Resource).Returns(newStateJObject);

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(response1.Object);
            _mockContainer.Setup(x => x.ReplaceItemAsync(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response2.Object);

            // Act
            var result = await _cosmosDbService.UpdateByIdAsync(tableName, "update", itemId, partitionKeyValue, updateValues, returnValueOption);

            // Assert
            Assert.AreEqual(newStateJObject, result.newState);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_UpdateByIdAsync_UpdatesItemAndReturnsOldState()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemId = "testItemId";
            var partitionKeyValue = "testPartitionKey";
            var updateValues = new Dictionary<string, object> { { "Property1", "UpdatedValue" } };
            var returnValueOption = ReturnValueOption.ALL_OLD;

            var oldStateJObject = new JObject { ["Property1"] = "OldValue" };
            var newStateJObject = new JObject { ["Property1"] = "UpdatedValue" };

            var response1 = new Mock<ItemResponse<JObject>>();
            response1.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response1.Setup(s => s.Resource).Returns(oldStateJObject);

            var response2 = new Mock<ItemResponse<JObject>>();
            response2.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response2.Setup(s => s.Resource).Returns(newStateJObject);

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(response1.Object);
            _mockContainer.Setup(x => x.ReplaceItemAsync(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response2.Object);

            // Act
            var result = await _cosmosDbService.UpdateByIdAsync(tableName, "update", itemId, partitionKeyValue, updateValues, returnValueOption);

            // Assert
            Assert.AreEqual(newStateJObject, result.oldState);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_UpdateByIdAsync_UpdatesItemAndReturnsOldAndNewState()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemId = "testItemId";
            var partitionKeyValue = "testPartitionKey";
            var updateValues = new Dictionary<string, object> { { "Property1", "UpdatedValue" } };
            var returnValueOption = ReturnValueOption.ALL_OLD_NEW;

            var oldStateJObject = new JObject { ["Property1"] = "OldValue" };
            var newStateJObject = new JObject { ["Property1"] = "UpdatedValue" };

            var response1 = new Mock<ItemResponse<JObject>>();
            response1.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response1.Setup(s => s.Resource).Returns(oldStateJObject);

            var response2 = new Mock<ItemResponse<JObject>>();
            response2.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response2.Setup(s => s.Resource).Returns(newStateJObject);

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(response1.Object);
            _mockContainer.Setup(x => x.ReplaceItemAsync(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response2.Object);

            // Act
            var result = await _cosmosDbService.UpdateByIdAsync(tableName, "update", itemId, partitionKeyValue, updateValues, returnValueOption);

            // Assert
            Assert.AreEqual(newStateJObject, result.oldState);
            Assert.AreEqual(newStateJObject, result.newState);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_UpdateByIdAsync_UpdatesItemAndReturnsNull()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemId = "testItemId";
            var partitionKeyValue = "testPartitionKey";
            var updateValues = new Dictionary<string, object> { { "Property1", "UpdatedValue" } };
            var returnValueOption = ReturnValueOption.NONE;

            var oldStateJObject = new JObject { ["Property1"] = "OldValue" };
            var newStateJObject = new JObject { ["Property1"] = "UpdatedValue" };

            var response1 = new Mock<ItemResponse<JObject>>();
            response1.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response1.Setup(s => s.Resource).Returns(oldStateJObject);

            var response2 = new Mock<ItemResponse<JObject>>();
            response2.Setup(s => s.StatusCode).Returns(HttpStatusCode.OK);
            response2.Setup(s => s.Resource).Returns(newStateJObject);

            _mockContainer.Setup(x => x.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(response1.Object);
            _mockContainer.Setup(x => x.ReplaceItemAsync(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response2.Object);

            // Act
            var result = await _cosmosDbService.UpdateByIdAsync(tableName, "update", itemId, partitionKeyValue, updateValues, returnValueOption);

            // Assert
            Assert.AreEqual(null, result.oldState);
            Assert.AreEqual(null, result.newState);
        }

        [Test]
        public void CosmosDbServiceUnitTests_UpdateByIdAsync_ItemNotFound_ThrowsCosmosException()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemId = "nonExistentItemId";
            var partitionKeyValue = "testPartitionKey";
            var updateValues = new Dictionary<string, object> { { "Property1", "UpdatedValue" } };
            var returnValueOption = ReturnValueOption.ALL_NEW;

            _mockContainer.Setup(c => c.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 404, "0", 0));

            // Act & Assert
            var ex = Assert.ThrowsAsync<CosmosException>(async () =>
                await _cosmosDbService.UpdateByIdAsync(tableName, "update", itemId, partitionKeyValue, updateValues, returnValueOption));

            Assert.AreEqual("Item not found", ex.Message);
        }

        [Test]
        public async Task CosmosDbServiceUnitTests_UpdateByIdAsync_HandlesExceptionAndLogsError()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbService, tableConfigMap);
            var itemId = "testItemId";
            var partitionKeyValue = "testPartitionKey";
            var updateValues = new Dictionary<string, object> { { "Property1", "UpdatedValue" } };
            var returnValueOption = ReturnValueOption.ALL_NEW;

            _mockContainer.Setup(c => c.ReadItemAsync<JObject>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("Database error", System.Net.HttpStatusCode.InternalServerError, 500, "0", 0));

            // Act
            var ex = Assert.ThrowsAsync<CosmosException>(async () =>
                await _cosmosDbService.UpdateByIdAsync(tableName, "update", itemId, partitionKeyValue, updateValues, returnValueOption));
        }
    }
}
