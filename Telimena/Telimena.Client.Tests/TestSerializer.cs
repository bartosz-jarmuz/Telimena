// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestSerializer
    {
        [Test]
        public void Test_Serializer()
        {

            var model = new UpdateRequest(23, "1.2.0", 99 ,true, "3.2.1.3");
        
            ITelimenaSerializer sut = new TelimenaSerializer();
            var stringified = sut.Serialize(model);
            var escaped = sut.UrlEncodeJson(stringified);

            Assert.AreEqual("{\"ProgramId\":23,\"UserId\":99,\"ProgramVersion\":\"1.2.0\",\"ToolkitVersion\":\"3.2.1.3\",\"AcceptBeta\":true}", stringified);
            Assert.AreEqual("%7B%22ProgramId%22%3A23%2C%22UserId%22%3A99%2C%22ProgramVersion%22%3A%221.2.0%22%2C%22ToolkitVersion%22%3A%223.2.1.3%22%2C%22AcceptBeta%22%3Atrue%7D", escaped);

            var unescaped = sut.UrlDecodeJson(escaped);
            Assert.AreEqual(stringified, unescaped);

            var objectified = sut.Deserialize<UpdateRequest>(unescaped);

            Assert.IsTrue(model.PublicInstancePropertiesAreEqual(objectified));
        }

    }
}