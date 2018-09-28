using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;

namespace Telimena.ToolkitClient.Serializer
{
    internal class NullPropertiesConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes => this.GetType().Assembly.GetTypes();

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
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
                    jsonExample.Add(prop.Name, value);
                }
            }

            return jsonExample;
        }
    }
}