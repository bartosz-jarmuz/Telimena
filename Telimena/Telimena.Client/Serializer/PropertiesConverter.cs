using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;

namespace TelimenaClient.Serializer
{
    internal class PropertiesConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes => this.GetType().Assembly.GetTypes().Where(
            x=> !x.IsEnum);

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException($"Deserialization is not implemented in {this.GetType().Name}");
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            Dictionary<string, object> jsonExample = new Dictionary<string, object>();
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                //check if decorated with ScriptIgnore attribute
                bool ignoreProp = prop.IsDefined(typeof(ScriptIgnoreAttribute), true);

                object value = prop.GetValue(obj, BindingFlags.Public, null, null, null);
                if (value != null && !ignoreProp)
                {
                    AssignValue(jsonExample, prop, value);
                }
            }

            return jsonExample;
        }

        private static void AssignValue(Dictionary<string, object> jsonExample, PropertyInfo prop, object value)
        {
            if (prop.PropertyType == typeof(DateTimeOffset))
            {
                jsonExample.Add(prop.Name, ((DateTimeOffset)value).ToString("O"));
            }
            else if (prop.PropertyType == typeof(DateTime))
            {
                jsonExample.Add(prop.Name, ((DateTime)value).ToString("O"));
            }
            else
            {
                jsonExample.Add(prop.Name, value);
            }
        }
    }
}