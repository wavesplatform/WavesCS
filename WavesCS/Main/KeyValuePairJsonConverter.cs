using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;

namespace WavesCS.Main
{
    class KeyValuePairJsonConverter : JavaScriptConverter
    {
            public override object Deserialize(IDictionary<string, object> deserializedJSObjectDictionary, Type targetType, JavaScriptSerializer javaScriptSerializer)
            {
                Object targetTypeInstance = Activator.CreateInstance(targetType);

                FieldInfo[] targetTypeFields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (FieldInfo fieldInfo in targetTypeFields)
                    fieldInfo.SetValue(targetTypeInstance, deserializedJSObjectDictionary[fieldInfo.Name]);

                return targetTypeInstance;
            }

            public override IDictionary<string, object> Serialize(Object objectToSerialize, JavaScriptSerializer javaScriptSerializer)
            {
                IDictionary<string, object> serializedObjectDictionary = new Dictionary<string, object>();

                FieldInfo[] objectToSerializeTypeFields = objectToSerialize.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (FieldInfo fieldInfo in objectToSerializeTypeFields)
                    serializedObjectDictionary.Add(fieldInfo.Name, fieldInfo.GetValue(objectToSerialize));

                return serializedObjectDictionary;
            }

            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    return new ReadOnlyCollection<Type>(new Type[] { typeof(Transaction) });
                }
            }           

    }

    public class Utils
    {
        public static void WriteToNetwork(System.IO.BinaryWriter writer, dynamic n)
        {
            n = System.Net.IPAddress.HostToNetworkOrder((long)n);
            writer.Write(n);
        }

        public static void WriteToNetwork(System.IO.BinaryWriter writer, short n)
        {
            byte[] shortN = BitConverter.GetBytes(n);
            Array.Reverse(shortN);
            writer.Write(shortN);          
        }
    }
}
