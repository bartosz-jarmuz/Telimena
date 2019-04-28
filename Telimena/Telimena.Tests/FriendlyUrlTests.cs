using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.Portal.Utils;
using Telimena.WebApp.Core.DTO;

namespace Telimena.Tests
{
    [TestFixture]
    public class FriendlyUrlTests
    {
        [Test]
        public void TestValidStrings()
        {
            this.EnsureIsValidAndRemainsUnchanged("ThisIsValid");
            this.EnsureIsValidAndRemainsUnchanged("Th34isIs34Va7lid");
            this.EnsureIsValidAndRemainsUnchanged("This-Is-Valid");
            this.EnsureIsValidAndRemainsUnchanged("EnemyNumber1");
            this.EnsureIsValidAndRemainsUnchanged("99problems");

        }

        [Test]
        public void TestInvalidStrings()
        {
            this.EnsureIsNotValidIsValidAfterwards("This Is Valid", "ThisIsValid");
            this.EnsureIsNotValidIsValidAfterwards("This-Is*Valid", "This-IsValid");
            this.EnsureIsNotValidIsValidAfterwards(" Valid", "Valid");
            this.EnsureIsNotValidIsValidAfterwards(" Valid ", "Valid");
            this.EnsureIsNotValidIsValidAfterwards("---Valid ", "Valid");
            this.EnsureIsNotValidIsValidAfterwards("---Valid- -", "Valid");

            this.EnsureIsNotValidIsValidAfterwards("ThisÓÓŻÓąćś", "This");

            Assert.IsFalse("---- -".IsUrlFriendly());
            Assert.IsNull("---- -".MakeUrlFriendly());

        }


        private void EnsureIsValidAndRemainsUnchanged(string input)
        {
            Assert.IsTrue(input.IsUrlFriendly());
            var output = input.MakeUrlFriendly();
            Assert.AreEqual(input, output);
        }

        private void EnsureIsNotValidIsValidAfterwards(string input, string expectedOutput)
        {
            Assert.IsFalse(input.IsUrlFriendly());
            var output = input.MakeUrlFriendly();
            Assert.IsTrue(output.IsUrlFriendly());

            Assert.AreEqual(expectedOutput, output);
        }

    }
}