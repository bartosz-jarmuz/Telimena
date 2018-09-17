namespace Telimena.Client
{
    public interface ITelimenaSerializer
    {
        T Deserialize<T>(string stringContent);
        string Serialize(object objectToPost);
        string UrlEncodeJson(string jsonString);
        string UrlDecodeJson(string escapedJsonString);
        string SerializeAndEncode(object objectToPost);
    }
}