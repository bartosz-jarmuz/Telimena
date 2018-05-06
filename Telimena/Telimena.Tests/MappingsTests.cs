namespace Telimena.Tests
{
    using NUnit.Framework;
    using WebApp;

    [TestFixture]
    public class MappingsTests
    {
        [Test]
        public void EnsureAutomapperConfigIsValid()
        {
           
            AutoMapperConfiguration.Validate();
        }

      
    }
}