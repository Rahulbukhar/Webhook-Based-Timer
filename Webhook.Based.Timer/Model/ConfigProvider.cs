namespace WebhookBasedTimer.Model
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly IConfiguration configuration;

        public ConfigProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // Get a specific configuration value by key
        public string GetConfig(string key)
        {
            return configuration[key];
        }

        // Get a strongly-typed configuration section
        public T GetConfig<T>(string sectionName) where T : new()
        {
            var section = new T();
            configuration.GetSection(sectionName).Bind(section);
            return section;
        }
    }
}
