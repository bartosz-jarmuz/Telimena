using System;
using System.Net.Http;
using Moq;
using NUnit.Framework;

namespace TelimenaClient.Tests
{
    internal static class Helpers
    {
        public static Mock<ITelimenaHttpClient> GetMockClient()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
            {
                throw new AggregateException(new AssertionException(uri), new AssertionException(content.ReadAsStringAsync().GetAwaiter().GetResult()));
            });
            return client;
        }

        public static void SetupMockHttpClient(ITelimena telimena, Mock<ITelimenaHttpClient> client)
        {
            ((Telimena) telimena).Messenger = new Messenger(((Telimena) telimena)?.Serializer, client.Object);
        }
    }
}