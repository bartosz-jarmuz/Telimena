namespace Telimena.Client
{
    internal interface ITelimenaSerializer
    {
        T Deserialize<T>(string stringContent);
        string Serialize(object objectToPost);
    }
}