namespace Subtext.Azure.Search.Services
{
    public interface ISerializationService
    {
        T Deserialize<T>(string content);

        string Serialize<T>(T item);
    }
}
