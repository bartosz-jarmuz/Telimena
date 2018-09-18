using DbIntegrationTestHelpers;
using NUnit.Framework;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.Tests
{
    [TestFixture]
    public class ToolkitUpdatesTests : IntegrationTestsContextSharedPerClass<TelimenaContext>
    {
        //The toolkit can evolve independently of the client app - some changes might be breaking the contracts, but most - should not
        //a non-breaking change example - add new functions or overloads, pull more client data or handle changes in the web api
        //a breaking change would be renaming methods, changing the default URI etc - these are the things that would break the app if DLL is replaced
        //in such cases, the app needs to be recompiled against latest toolkit.

        //todo - IF an update request comes from assembly that does not support latest telimena yet, but the update packages do, then it should all be downloaded at the same time
        private void Seed()
        {
        }

        [Test]
        public void Test_ComplexScenario()
        {
            //how is tookit developed & updated?
            //when the changes are made to Telimena.Client, they will be pushed to Nuget for devs to use
            //however, the .dll should also be uploaded to Telimena.Portal,
            //because the client applications might use new version without recompilation (& without original developer involvement!)

            //Todo - an update request comes from a program FOOBARER v.4.0 which uses toolkit v1.0
            //there were several changes of the toolkit since that time - 1.1, 1.2, 1.3 and so on up to 2.0
            //1.4 was marked as 'introducing breaking changes'
            //each of the program & update packages needs to have a version of toolkit it was compiled against stored in DB
            //the FOOBARER v.4.0 was compiled against toolkit 1.0
            //(we get that info by, either prompting the dev when package is uploaded (MVP), or verifying automatically)
            // based on the request we can figure out that we can grab new toolkit v.1.3 (because 1.4 is breaking)

            //todo - meanwhile, when a new toolkit version is uploaded and is marked as 'breaking changes'
            //all the programs which were not compiled against that version should get a notification:
            // - at developer portal, a box saying '4 of your apps need to be recompiled to use latest Telimena
            //The dev should be able to open project in VS, update nuget and recompile
            //then go to portal and upload update package - FOOBARER v.4.1
            //When uploading an update, dev is prompted for version of toolkit that was used (later this becomes automatic)

            //next time an update request comes from a program FOOBARER v.4.0 which uses toolkit v1.0 (or 1.3 at this point)
            //it will recognize that there is an incompatible update for the toolkit, because v.4.0 is not compatible with toolkit 1.4,
            //BUT ALSO an update for the app 
            //the update of the application will contain the latest compatible Toolkit, based on what toolkit was the update compiled against
            //TBD - should the toolkit come from the update package or separately?

        
        }

        [Test]
        public void Test_SimpleScenario()
        {
            //Todo - an update request comes from a program which uses toolkit 1.0
            //there were several changes of the toolkit since that time - 1.1, 1.2, 1.3
            //none of them is marked as 'introduces breaking changes'
            //regardless whether there is an app update or not, we can prompt that an update is available, and download the toolkit 1.3
        }
    }
}