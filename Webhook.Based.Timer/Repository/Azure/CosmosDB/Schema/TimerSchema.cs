

using NoSqlDataAccess.Common.Provisioning.Model;
using WebhookBasedTimer.Model;

namespace DatasetService.Core.Repository.Azure.CosmosDB.Schema
{

    /// <summary>
    /// This class provides data model for Timer.
    /// </summary>
    internal static class TimerSchema
    {
        /// <summary>
        /// Schema
        /// </summary>
        public static readonly ContainerSchema Schema = CreateContainerSchema();

        private static ContainerSchema CreateContainerSchema()
        {
            ContainerSchema schema = new()
            {
                Attributes = new List<ContainerAttribute>
                {
                    new() { Name = nameof(TimerInstance.Id), Type = ContainerAttributeType.String },
                    new() { Name = nameof(TimerInstance.DateCreated), Type = ContainerAttributeType.DateTime },
                    new() { Name = nameof(TimerInstance.Hours), Type = ContainerAttributeType.Number },
                    new() { Name = nameof(TimerInstance.Minutes), Type = ContainerAttributeType.Number },
                    new() { Name = nameof(TimerInstance.Seconds), Type = ContainerAttributeType.Number },
                    new() { Name = nameof(TimerInstance.TimeLeft), Type = ContainerAttributeType.Number },
                    new() { Name = nameof(TimerInstance.WebhookUrl), Type = ContainerAttributeType.String },
                    new() { Name = nameof(TimerInstance.Status), Type = ContainerAttributeType.String }
                },
                CosmosDBKeyConfiguration = new CosmosDBKeyConfiguration
                {
                    PartitionKeyPath = $"/{"userId"}"
                }
            };

            return BaseSchema.AddCommonContainerAttributes(schema);
        }

    }
}
