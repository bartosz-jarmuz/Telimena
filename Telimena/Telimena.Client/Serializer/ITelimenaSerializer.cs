namespace Telimena.Client
{
    public interface ITelimenaSerializer
    {
        T Deserialize<T>(string stringContent);
        string Serialize(object objectToPost);
        string SerializeAndEncode(object objectToPost);
        string UrlDecodeJson(string escapedJsonString);
        string UrlEncodeJson(string jsonString);
    }
}