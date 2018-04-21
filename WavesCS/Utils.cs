using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using System.IO;
using System.Web.Script.Serialization;

namespace WavesCS
{
    class KeyValuePairJsonConverter : JavaScriptConverter
    {
            public override object Deserialize(IDictionary<string, object> deserializedJsObjectDictionary, Type targetType, JavaScriptSerializer javaScriptSerializer)
            {
                var targetTypeInstance = Activator.CreateInstance(targetType);
                var targetTypeFields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (var fieldInfo in targetTypeFields)
                    fieldInfo.SetValue(targetTypeInstance, deserializedJsObjectDictionary[fieldInfo.Name]);

                return targetTypeInstance;
            }

            public override IDictionary<string, object> Serialize(Object objectToSerialize, JavaScriptSerializer javaScriptSerializer)
            {
                var serializedObjectDictionary = new Dictionary<string, object>();

                var objectToSerializeTypeFields = objectToSerialize.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (var fieldInfo in objectToSerializeTypeFields)
                    serializedObjectDictionary.Add(fieldInfo.Name, fieldInfo.GetValue(objectToSerialize));

                return serializedObjectDictionary;
            }

            public override IEnumerable<Type> SupportedTypes => new ReadOnlyCollection<Type>(new[] { typeof(Transaction) });
    }

    public static class Utils
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        
        public static void WriteLong(this BinaryWriter writer, long n)
        {
            n = System.Net.IPAddress.HostToNetworkOrder(n);
            writer.Write(n);
        }

        public static void WriteShort(this BinaryWriter writer, short n)
        {
            byte[] shortN = BitConverter.GetBytes(n);
            Array.Reverse(shortN);
            writer.Write(shortN);          
        }

        public static long CurrentTimestamp()
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            return (DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond * 1000;
        }

        public static string ToJson(this Dictionary<string, object> data)
        {
            return serializer.Serialize(data);
        }
    }
}
