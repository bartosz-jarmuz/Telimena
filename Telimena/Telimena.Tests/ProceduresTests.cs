using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Telimena.WebApp.Infrastructure.Database.Sql.StoredProcedures;

namespace Telimena.Tests
{
    [TestFixture]
    public class ProceduresTests
    {
        [Test]
        public void ValidateProcedureNames()
        {
            var dire = TestContext.CurrentContext.TestDirectory;
            var path = Path.Combine(dire, "Database\\Sql\\StoredProcedures");
            DirectoryInfo dir = new DirectoryInfo(path);
            var spFiles = dir.GetFiles("*.sql");
            foreach (FileInfo file in spFiles)
            {
                string spName = Path.GetFileNameWithoutExtension(file.Name);
                var fileContent = File.ReadAllText(file.FullName);
                Assert.IsTrue(Regex.IsMatch( fileContent, 
                        ($@"CREATE PROCEDURE (\[dbo\]\.)?(\[{spName}\]){{1}}"), RegexOptions.IgnoreCase), $"Incorrect declaration of SP {spName}. " +
                                                                                                          $"Procedure name must match file name without extension. SP Text:\r\n{fileContent}");

                var prop = typeof(StoredProcedureNames).GetProperty(spName);
                Assert.IsNotNull(prop, $"{nameof(StoredProcedureNames)} class does not contain a property {spName}");
                Assert.AreEqual(spName, prop.GetValue(null));
            }
        }

        [Test]
        public void ValidateFunctionNames()
        {
            var dire = TestContext.CurrentContext.TestDirectory;
            var path = Path.Combine(dire, "Database\\Sql\\Functions");
            DirectoryInfo dir = new DirectoryInfo(path);
            var spFiles = dir.GetFiles("*.sql");
            foreach (FileInfo file in spFiles)
            {
                string funcName = Path.GetFileNameWithoutExtension(file.Name);
                var fileContent = File.ReadAllText(file.FullName);
                Assert.IsTrue(Regex.IsMatch(fileContent,
                        ($@"CREATE FUNCTION (\[dbo\]\.)?(\[{funcName}\]){{1}}"), RegexOptions.IgnoreCase), $"Incorrect declaration of func {funcName}. " +
                                                                                                          $"Func name must match file name without extension. SP Text:\r\n{fileContent}");
            }
        }

    }
}