using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedLogic;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;
using TelimenaClient.Model;

namespace Telimena.TestUtilities.Base
{
    [TestFixture]
    public abstract partial class IntegrationTestBase 
    {
        public readonly string AdminName = ConfigHelper.GetSetting<string>(ConfigKeys.AdminName);
        public readonly string User1Name = ConfigHelper.GetSetting(ConfigKeys.User1Name);
        public readonly string User2Name = ConfigHelper.GetSetting(ConfigKeys.User2Name);
        public readonly string User3Name = ConfigHelper.GetSetting(ConfigKeys.User3Name);
        public readonly string AdminPassword = ConfigHelper.GetSetting(ConfigKeys.AdminPassword);
        public readonly string User1Password = ConfigHelper.GetSetting(ConfigKeys.User1Password);
        public readonly string User2Password = ConfigHelper.GetSetting(ConfigKeys.User2Password);
        public readonly string User3Password = ConfigHelper.GetSetting(ConfigKeys.User3Password);
        protected readonly bool IsLocalTestSetting = ConfigHelper.GetSetting<bool>(ConfigKeys.IsLocalTest);

    }
}