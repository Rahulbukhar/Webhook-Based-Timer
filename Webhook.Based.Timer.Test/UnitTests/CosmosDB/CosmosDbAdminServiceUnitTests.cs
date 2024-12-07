

using Moq;
using NUnit.Framework;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Net;

namespace NoSqlDataAccess.CosmosDB.Tests
{  
    using NoSqlDataAccess.Azure.CosmosDB.DBService;
    using NoSqlDataAccess.Common.Provisioning.Model;
    using NoSqlDataAccess.Common.Provisioning;
    using NoSqlDataAccess.Common.Constants;

    [TestFixture]
    [Category("TestSuite.Unit")]
    public class CosmosDbAdminServiceTests
    {
        private Mock<ICosmosDbConnection> _mockCosmosDbConnection;
        private Mock<IAzureCosmosDbClientFactory> _mockAzureCosmosDbClientFactory;
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Database> _mockDatabase;
        private Mock<Container> _mockContainer;
        private CosmosDbAdminService _cosmosDbAdminService;

        [SetUp]
        public void SetUp()
        {
            // Mock the dependencies
            _mockCosmosDbConnection = new Mock<ICosmosDbConnection>();
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockDatabase = new Mock<Database>();
            _mockContainer = new Mock<Container>();

            // Set up mock configuration values
            _mockCosmosDbConnection.Setup(c => c.EndpointUri).Returns("https://mock-cosmosdb-endpoint/");
            _mockCosmosDbConnection.Setup(c => c.PrimaryKey).Returns("MY_TEST_KEY");
            _mockCosmosDbConnection.Setup(c => c.DatabaseName).Returns("TestDatabase");
            _mockCosmosDbConnection.Setup(c => c.ProvisioningConfig).Returns(new Mock<ICosmosDbProvisioningConfiguration>().Object);

            _mockAzureCosmosDbClientFactory = new Mock<IAzureCosmosDbClientFactory>();

            _mockAzureCosmosDbClientFactory.Setup(c => c.GetCosmosDbClient(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_mockCosmosClient.Object);

            _mockCosmosClient.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(_mockDatabase.Object);

            _cosmosDbAdminService = new CosmosDbAdminService(_mockCosmosDbConnection.Object, _mockAzureCosmosDbClientFactory.Object, new Mock<ICosmosDbProvisioningConfiguration>().Object);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_CreateTableAsync_ShouldReturnTableCreated_WhenTableDoesNotExist()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Act
            var result = await _cosmosDbAdminService.CreateTableAsync(tableName);

            // Assert
            Assert.That(new string(TableConstants.TABLE_CREATED), Is.EqualTo(result));
            _mockCosmosClient.Verify(client => client.GetDatabase(It.IsAny<string>()), Times.Once);
            _mockDatabase.Verify(client => client.CreateContainerAsync(It.IsAny<ContainerProperties>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_CreateTableAsync_ShouldReturnTableConfigNotFound_WhenTableConfigDoesNotExist()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
                {
                    { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
                };

            // Mock the table configuration lookup
            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Act
            var result = await _cosmosDbAdminService.CreateTableAsync("Test1");

            // Assert
            Assert.That(new string(TableConstants.TABLE_CONFIG_NOT_FOUND), Is.EqualTo(result));
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_CreateTableAsync_ShouldReturnAlreadyExists_WhenTableExist()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Mock the CreateContainerAsync to throw a conflict exception
            _mockDatabase.Setup(s => s.CreateContainerAsync(It.IsAny<ContainerProperties>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("Conflict", HttpStatusCode.Conflict, 4, String.Empty, 5));

            // Act
            var result = await _cosmosDbAdminService.CreateTableAsync(tableName);

            // Assert
            Assert.AreEqual(TableConstants.TABLE_ALREADY_EXIST, result);
            _mockCosmosClient.Verify(client => client.GetDatabase(It.IsAny<string>()), Times.Once);
            _mockDatabase.Verify(client => client.CreateContainerAsync(It.IsAny<ContainerProperties>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_CreateTableAsync_ShouldReturnError()
        {
            // Arrange
            var tableName = "TestTable";
            string result = "";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Mock the CreateContainerAsync to throw a conflict exception
            _mockDatabase.Setup(s => s.CreateContainerAsync(It.IsAny<ContainerProperties>(), It.IsAny<int?>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Failed to create table"));

            // Act
            try
            {
                result = await _cosmosDbAdminService.CreateTableAsync(tableName);
            }
            catch (Exception ex)
            {
                // Assert               
                _mockCosmosClient.Verify(client => client.GetDatabase(It.IsAny<string>()), Times.Once);
            }
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_DescribeTablesAsync_ShouldReturnValidContainerResponse()
        {
            // Arrange
            var tableNames = new List<string> { "Table1" };
            var mockTableProperties1 = new ContainerProperties("Table1", "/partitionKey1")
            {
                IndexingPolicy = new IndexingPolicy(),
                UniqueKeyPolicy = new UniqueKeyPolicy()
            };
            // Set up the mock container responses
            Mock<ContainerResponse> mockResponse = new Mock<ContainerResponse>();

            mockResponse.Setup(s => s.Resource).Returns(mockTableProperties1).Verifiable();

            _mockDatabase.Setup(d => d.GetContainer("Table1")).Returns(_mockContainer.Object);
            _mockContainer.Setup(c => c.ReadContainerAsync(It.IsAny<ContainerRequestOptions>(), default))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await _cosmosDbAdminService.DescribeTablesAsync(tableNames);

            // Assert
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0]["id"].ToString(), "Table1");
            Assert.AreEqual(result[0]["partitionKeyPath"].ToString(), "/partitionKey1");
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_DescribeTablesAsync_ShouldReturnException_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableNames = new List<string> { "Table1" };
            // Set up the mock container responses

            _mockDatabase.Setup(d => d.GetContainer("Table1")).Throws(new Exception("Failed to describe table"));
            // Act
            var result = await _cosmosDbAdminService.DescribeTablesAsync(tableNames);

            // Assert
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_UpdateTimeToLiveAsync_ShouldReturn_TTL_NOT_CONFIGURED_When_CorrectTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Act
            var result = await _cosmosDbAdminService.UpdateTimeToLiveAsync(tableName);
            Assert.AreEqual(result, TableConstants.TTL_NOT_CONFIGURED);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_UpdateTimeToLiveAsync_ShouldReturn_TABLE_NOT_CONFIGURED_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } } } }
            };

            // Mock the table configuration lookup
            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Act
            var result = await _cosmosDbAdminService.UpdateTimeToLiveAsync("Table1");
            Assert.AreEqual(result, TableConstants.TABLE_NOT_FOUND);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_UpdateTimeToLiveAsync_ShouldReturn_TTL_UPDATED_When_CorrectTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            Mock<ContainerResponse> mockResponse = new Mock<ContainerResponse>();
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } },CosmosDBTimeToLiveConfig = new CosmosDBTimeToLiveConfig{ TtlInSeconds = 10 } } }
            };

            var mockTableProperties1 = new ContainerProperties("TestTable", "/partitionKey1")
            {
                IndexingPolicy = new IndexingPolicy(),
                UniqueKeyPolicy = new UniqueKeyPolicy()
            };

            // Set up the mock container responses
            _mockDatabase.Setup(d => d.GetContainer("TestTable")).Returns(_mockContainer.Object);

            mockResponse.Setup(s => s.Resource).Returns(mockTableProperties1).Verifiable();
            _mockContainer.Setup(c => c.ReadContainerAsync(It.IsAny<ContainerRequestOptions>(), default))
                .ReturnsAsync(mockResponse.Object);

            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Act
            var result = await _cosmosDbAdminService.UpdateTimeToLiveAsync(tableName);
            Assert.AreEqual(result, TableConstants.TTL_UPDATED);
        }


        [Test]
        public async Task CosmosDbAdminServiceTests_UpdateTimeToLiveAsync_ShouldThrow_Exception_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            string result = string.Empty;
            Mock<ContainerResponse> mockResponse = new Mock<ContainerResponse>();
            var tableConfigMap = new Dictionary<string, ContainerConfiguration>
            {
                { tableName, new ContainerConfiguration { Schema = new ContainerSchema { CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration { PartitionKeyPath = "/id" } },CosmosDBTimeToLiveConfig = new CosmosDBTimeToLiveConfig{ TtlInSeconds = 10 } } }
            };

            var mockTableProperties1 = new ContainerProperties("TestTable", "/partitionKey1")
            {
                IndexingPolicy = new IndexingPolicy(),
                UniqueKeyPolicy = new UniqueKeyPolicy()
            };

            // Set up the mock container responses
            _mockDatabase.Setup(d => d.GetContainer("TestTable")).Throws(new Exception("Failed to Update Table"));

            mockResponse.Setup(s => s.Resource).Returns(mockTableProperties1).Verifiable();


            typeof(CosmosDbAdminService)
                .GetField("tableConfigMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_cosmosDbAdminService, tableConfigMap);

            // Act
            try
            {
                result = await _cosmosDbAdminService.UpdateTimeToLiveAsync(tableName);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(result, string.Empty);
            }
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_DeleteContainerAsync_ShouldReturn_TABLE_DELETED_When_CorrectTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";

            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Returns(_mockContainer.Object);

            // Act
            var result = await _cosmosDbAdminService.DeleteContainerAsync(tableName);
            Assert.AreEqual(result, TableConstants.TABLE_DELETED);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_DeleteContainerAsync_ShouldReturn_Exception_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            string result = string.Empty;

            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Throws(new Exception("Failed to Delete Table"));

            // Act
            try
            {
                result = await _cosmosDbAdminService.DeleteContainerAsync(tableName);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(result, string.Empty);
            }
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_DeleteContainerAsync_ShouldReturn_CosmosException_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";

            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Throws(new CosmosException("Not found", HttpStatusCode.NotFound, 4, String.Empty, 5));

            // Act
            var result = await _cosmosDbAdminService.DeleteContainerAsync(tableName);
            Assert.AreEqual(result, TableConstants.TABLE_NOT_FOUND);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_GetContainerThroughputAsync_ShouldReturn_Throughput_When_CorrectTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";

            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Returns(_mockContainer.Object);
            _mockContainer.Setup(c => c.ReadThroughputAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            // Act
            var result = await _cosmosDbAdminService.GetContainerThroughputAsync(tableName);
            Assert.AreEqual(result, 10);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_GetContainerThroughputAsync_ShouldReturn_CosmosException_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            int result = 0;
            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Throws(new Exception("Table Not Found"));

            // Act
            try
            {
                result = await _cosmosDbAdminService.GetContainerThroughputAsync(tableName);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(result, 0);
            }
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_UpdateContainerThroughputAsync_ShouldReturn_THROUGHPUT_UPDATED_When_CorrectTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";

            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Returns(_mockContainer.Object);

            // Act
            var result = await _cosmosDbAdminService.UpdateContainerThroughputAsync(tableName, 10);
            Assert.AreEqual(result, TableConstants.THROUGHPUT_UPDATED);
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_UpdateContainerThroughputAsync_ShouldReturn_Exception_When_WrongTableNameIsPassed()
        {
            // Arrange
            var tableName = "TestTable";
            string result = string.Empty;

            // Mocks
            _mockDatabase.Setup(d => d.GetContainer(tableName)).Throws(new Exception("Failed to Delete Table"));

            try
            {
                result = await _cosmosDbAdminService.UpdateContainerThroughputAsync(tableName, 10);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(result, string.Empty);
            }
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_ListContainersAsync_ShouldReturn_List()
        {
            // Arrange
            var tableName = "TestTable";

            // Mocks
            Mock<FeedIterator<ContainerProperties>> mockContainerProperties = new Mock<FeedIterator<ContainerProperties>>();

            var containerPropertiesList =
                new List<ContainerProperties>
                {
                   new
                   ContainerProperties
                   {
                       Id = tableName
                   }
                };

            var mockFeedResponse = new Mock<FeedResponse<ContainerProperties>>();
            mockFeedResponse.Setup(r => r.GetEnumerator()).Returns(containerPropertiesList.GetEnumerator());

            _mockDatabase.Setup(s => s.GetContainerQueryIterator<ContainerProperties>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Returns(mockContainerProperties.Object);

            mockContainerProperties.SetupSequence(i => i.HasMoreResults)
            .Returns(true)
            .Returns(false);

            mockContainerProperties.Setup(s => s.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFeedResponse.Object);

            // Act
            var result = await _cosmosDbAdminService.ListContainersAsync();
            // Assert
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].ToString(), "TestTable");
        }

        [Test]
        public async Task CosmosDbAdminServiceTests_ListContainersAsync_ShouldReturn_EmptyList()
        {
            // Arrange
            var tableName = "TestTable";
            List<string> result = new List<string>();
            Mock<FeedIterator<ContainerProperties>> mockContainerProperties = new Mock<FeedIterator<ContainerProperties>>();

            Mock<FeedResponse<ContainerProperties>> mockFeedResponse = new Mock<FeedResponse<ContainerProperties>>();

            _mockDatabase.Setup(s => s.GetContainerQueryIterator<ContainerProperties>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
                .Throws(new Exception("Failed to get the list of the tables"));

            // Act
            try
            {
                result = await _cosmosDbAdminService.ListContainersAsync();
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(result.Count, 0);
            }
        }
    }
}