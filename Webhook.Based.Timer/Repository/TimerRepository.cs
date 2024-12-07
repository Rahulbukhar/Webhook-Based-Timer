using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace WebhookBasedTimer.Repository
{
    using Model;
    using DatasetService.Core.Repository.Azure.CosmosDB.Schema;
    using NoSqlDataAccess.Common;
    using NoSqlDataAccess.Common.Model;
    using NoSqlDataAccess.Common.Query;

    public class TimerRepository : ITimerRepository
    {
        /// <summary>
        /// NoSql DB service.
        /// </summary>
        protected readonly INoSqlDbService dbService;

        public TimerRepository(INoSqlDbService dbService)
        {
            this.dbService = dbService;
        }

        public async Task<JObject> CreateTimerAsync(TimerInstance timer)
        {
            var input = JObject.FromObject(timer);
            var response = await dbService.CreateObjectsAsync(ContainerNames.TIMER_CONTAINER_NAME, new List<JObject> { input });
            return response.FirstOrDefault();
        }

        public async Task<JObject> UpdateTimerAsync(TimerInstance timer)
        {
            var input = JObject.FromObject(timer);
            var updateValues = new Dictionary<string, object>
            {
                { "timeLeft", timer.TimeLeft },
                { "isTriggered", timer.IsTriggered },
                { "status", timer.Status },
                { "isActive", timer.IsActive }
            };
            var response = await dbService.UpdateByIdAsync(ContainerNames.TIMER_CONTAINER_NAME, null, timer.Id, "TenantUser", updateValues, ReturnValueOption.ALL_NEW);
            return response.newState;
        }

        public async Task<TimerInstance> GetTimerAsync(string id)
        {
            try
            {
                var response = await dbService.FindByIdsAsync(ContainerNames.TIMER_CONTAINER_NAME, new List<string> { id }, "TenantUser", null);
                return response.FirstOrDefault()?.ToObject<TimerInstance>();
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<TimerInstance>> GetAllTimersAsync()
        {
            try
            {
                List<JObject> response = await dbService.FindBySearchCriteriaAsync(ContainerNames.TIMER_CONTAINER_NAME, null, "TenantUser", null, null, "ServiceUser");
                
                return response?.Select(item => item.ToObject<TimerInstance>()) ?? Enumerable.Empty<TimerInstance>();
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error fetching timers: {ex.Message}");
                throw;
            }
        }


        public async Task<List<JObject>> GetTimersWithTimeLeftAsync(DateTime currentTime)
        {
            var rawJson = $@"{{
                'op' : 'And',
                'searchTerms': [
                    {{
                        'op': 'Eq',
                        'left': 'isActive',
                        'right': true
                    }}
                ]
            }}";
            var jsonData = JToken.Parse(rawJson);
            var searchCriteria = QueryExpressionParser.Parse(jsonData);
            var userId = "ServiceUser";

            return await dbService.FindBySearchCriteriaAsync(ContainerNames.TIMER_CONTAINER_NAME, null, "TenantUser", searchCriteria, null, userId);
        }


    }
}
