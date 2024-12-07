namespace WebhookBasedTimer.Model
{
    public interface IConfigProvider
    {
        string GetConfig(string key);
        T GetConfig<T>(string sectionName) where T : new();
    }
}
