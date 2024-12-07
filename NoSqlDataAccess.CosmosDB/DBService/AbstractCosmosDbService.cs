

using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlDataAccess.Azure.CosmosDB.DBService
{
    using Common.Provisioning;
    using Common.Provisioning.Model;
    using Common.Constants.Internal;
    using NoSqlDataAccess.Common.Extensions;
    using System.Threading.Tasks;

    /// <summary>
    /// Abstract functionality of CosmosDB service.
    /// </summary>
    public class AbstractCosmosDbService
    {

        /// <summary>
        /// An instance of <see cref="CosmosClient"/>.
        /// </summary>
        private CosmosClient cosmosDbClient;

        /// <summary>
        /// An instance of <see cref="CosmosClient"/>.
        /// </summary>
        protected CosmosClient CosmosDbClient
        {
            get
            {
                FetchCosmosDbClient().GetAwaiter().GetResult();
                return cosmosDbClient;
            }
        }

        /// <summary>
        /// Name of the table that stores query tokens.
        /// </summary>
        protected readonly string queryTokensContainerName;

        /// <summary>
        /// Name of the database.
        /// </summary>
        protected readonly string DatabaseName;

        /// <summary>
        /// Name of the PrimaryKey.
        /// </summary>
        protected readonly string PrimaryKey;

        /// <summary>
        /// Map of table names and provisioning configuration.
        /// </summary>
        protected Dictionary<string, ContainerConfiguration> tableConfigMap;

        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        private static readonly JsonSerializerSettings jsSettings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };

        /// <summary>
        /// Instance of AzureCosmosDbClientFactory.
        /// </summary>
        private readonly IAzureCosmosDbClientFactory cosmosDbClientFactory;

        /// <summary>
        /// URL of the Azure CosmosDB Service.
        /// </summary>
        private readonly string serviceUrl;
        /// <summary>
        /// Constructor
        /// </summary>

        /// <summary>
        /// CosmosDB System Fields.
        /// </summary>
        private static readonly List<string> fieldsToRemove = new List<string> { "_rid", "_self", "_etag", "_attachments", "_ts" };

        /// <summary>
        /// Constructor for AbstractCosmosDbService
        /// </summary>
        public AbstractCosmosDbService(ICosmosDbConnection cosmosDbConnection, IAzureCosmosDbClientFactory cosmosDbClientFactory, ICosmosDbProvisioningConfiguration cosmosDbProvisioning)
        {
            this.cosmosDbClientFactory = cosmosDbClientFactory;
            serviceUrl = cosmosDbConnection.EndpointUri ?? Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!;
            DatabaseName = cosmosDbConnection.DatabaseName ?? Environment.GetEnvironmentVariable("COSMOS_DATABASE_NAME")!;
            PrimaryKey = cosmosDbConnection.PrimaryKey ?? Environment.GetEnvironmentVariable("COSMOS_KEY")!;

            FetchCosmosDbClient().GetAwaiter().GetResult();

            if (cosmosDbProvisioning != null)
            {
                PopulateContainerConfigurations(cosmosDbProvisioning);
            }
        }

        /// <summary>
        /// Fetches CosmosDB Client.
        /// </summary>
        /// <returns>A <see cref="Task"/> which on completion would fetch the Cosmos Db client.</returns>
        private async Task FetchCosmosDbClient()
        {
            try
            {
                cosmosDbClient = await cosmosDbClientFactory.GetCosmosDbClient(serviceUrl, PrimaryKey);
                Console.WriteLine($"Instantiated CosmosDB Client with service URL: {serviceUrl}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"CosmosDB Client Creation Failed: {e.Message}");
            }
        }

        /// <summary>
        /// Populates <see cref="tableConfigMap"/>.
        /// </summary>
        /// <param name="cosmosDbProvisionConfig">An instance of <see cref="ICosmosDbProvisioningConfiguration"/>.</param>
        private void PopulateContainerConfigurations(ICosmosDbProvisioningConfiguration cosmosDbProvisionConfig)
        {
            var tableConfigs = cosmosDbProvisionConfig.ContainerConfigurations;
            if (tableConfigs != null)
            {
                tableConfigMap = tableConfigs.ToDictionary(entry => entry.ContainerName, entry => entry);
            }
        }

        /// <summary>
        /// Converts items returned from CosmosDB into JSON objects.
        /// </summary>
        /// <param name="tableConfig">The table config.</param>
        /// <param name="tableResponse">Map key and value response data.</param>
        /// <returns>An instance of <see cref="List{JObject}"/>.</returns>
        protected List<JObject> ConvertCosmosDbItems(ContainerConfiguration tableConfig, List<JObject> tableResponse)
        {
            var output = new List<JObject>();

            if (!tableResponse.IsNullOrEmpty())
            {
                foreach (var item in tableResponse)
                {
                    output.Add(ConvertCosmosDbItemToJObject(item, tableConfig));
                }
            }

            return output;
        }

        /// <summary>
        /// Converts a CosmosDB item to a JObject.
        /// </summary>
        /// <param name="item">DynamoDB item as a dictionary.</param>
        /// <param name="tableConfig">The Custom Id Processing Check.</param>
        /// <returns>An instance of <see cref="JObject"/>.</returns>
        protected JObject ConvertCosmosDbItemToJObject(JObject item, ContainerConfiguration tableConfig)
        {
            var cleanedItem = new JObject(item);

            foreach (var field in fieldsToRemove)
            {
                cleanedItem.Remove(field);
            }

            if (tableConfig.HandleIdFieldProcessing)
            {
                var dbNameToNameMap = tableConfig.Schema.Attributes
                    .Where(attr => attr.DBName != null)
                    .ToDictionary(attr => attr.DBName, attr => attr.Name);

                // Modify fields in-place: rename dbName fields back to original names
                var keys = cleanedItem.Properties().Select(p => p.Name).ToList();
                foreach (var key in keys)
                {
                    if (dbNameToNameMap.TryGetValue(key, out var originalName))
                    {
                        var value = cleanedItem[key];
                        cleanedItem.Remove(key);
                        cleanedItem[originalName] = value;
                    }
                }
            }

            string serialized = JsonConvert.SerializeObject(cleanedItem, jsSettings);

            return JsonConvert.DeserializeObject<JObject>(serialized, jsSettings);
        }
    }
}
