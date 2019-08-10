using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Telimena.TestUtilities.Base;

namespace Telimena.TestUtilities
{
    public static class ConfigHelper
    {
        public static string GetSetting(string key)
        {
            if (NUnit.Framework.TestContext.Parameters.Count == 0)
            {
                return TryGetSettingFromXml(key);
            }
            var x = NUnit.Framework.TestContext.Parameters[key];
            return x;
        }

        private static string TryGetSettingFromXml(string key)
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            var file = dir.GetFiles("*.runsettings", SearchOption.AllDirectories).FirstOrDefault();
            if (file != null)
            {
                XDocument xDoc = XDocument.Load(file.FullName);
                var ele = xDoc.Root.Element("TestRunParameters").Elements().FirstOrDefault(x => x.Attribute("name")?.Value == key);
                return ele?.Attribute("value")?.Value;
            }

            return null;
        }

        public static TimeSpan GetTimeout()
        {
            return TimeSpan.FromSeconds(GetSetting<int>(ConfigKeys.WaitTimeoutSeconds));
        }

        public static T GetSetting<T>(string key)
        {
            string val = GetSetting(key);
            if (val == null)
            {
                throw new ArgumentException($"Missing setting: {key}");
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }

        public static T GetSetting<T>(string key, T defaultValue)
        {
            string val = GetSetting(key);
            if (val == null)
            {
                return defaultValue;
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }
    }
}