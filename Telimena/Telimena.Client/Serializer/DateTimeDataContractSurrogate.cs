using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace TelimenaClient.Serializer
{
    internal class DateTimeDataContractSurrogate : IDataContractSurrogate
    {

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            // not used
            return null;
        }

        public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
        {
            // not used
            return null;
        }

        public Type GetDataContractType(Type type)
        {
            // not used
            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            if (obj.GetType() == typeof(List<object>))
            {
                List<object> inputCollection = (List<object>)obj;
                List<object> outputCollection = new List<object>(); 
                foreach (object item in inputCollection)
                {
                    if (item is string s)
                    {
                        if (DateTime.TryParseExact(s, "O", null, DateTimeStyles.None, out DateTime dt))
                        {
                            outputCollection.Add(dt);
                            continue;
                        }
                    }
                    outputCollection.Add(item); 
                }
                return outputCollection;
            }
            if (obj.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> inputCollection = (Dictionary<string, object>)obj;
                Dictionary<string, object> outputCollection = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> item in inputCollection)
                {
                    if (item.Value is string s)
                    {
                        DateTimeStyles style = s.EndsWith("Z") ? DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
                        if (DateTime.TryParseExact(s, "O", null, style, out DateTime dt))
                        {
                            outputCollection.Add(item.Key, dt);
                            continue;
                        }
                    }
                    outputCollection.Add(item.Key, item.Value); 
                }
                return outputCollection;
            }

            return obj;
        }

        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
        {
            // not used   
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            // for debugging
            return obj;
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            // not used
            return null;
        }

        public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
        {
            // not used
            return typeDeclaration;
        }
    }
}