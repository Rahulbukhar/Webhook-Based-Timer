

using NUnit.Framework;
using NoSqlDataAccess.Azure.CosmosDB.DBService;
using NoSqlDataAccess.Azure.CosmosDB.DBService.Internal;

namespace NoSqlDataAccess.CosmosDB.Test.UnitTests
{
    [TestFixture]
    [Category("TestSuite.Unit")]
    public class AzureCosmosDbClientFactoryUnitTests
    {
        private IAzureCosmosDbClientFactory _azureCosmosDbClientFactory;

        [SetUp]
        public void Setup()
        {
            _azureCosmosDbClientFactory = new AzureCosmosDbClientFactory();
        }

        [Test]
        public void GetCosmosDbClient_ShouldReturnValidClient()
        {
        }

        [Test]
        public void GetCosmosDbClient_WithInvalidParameters_ShouldThrowException()
        {
        }
    }
}
