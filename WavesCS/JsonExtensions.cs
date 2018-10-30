using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace WavesCS
{
    public static class JsonExtensions
    {        
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        
        public static string ToJson(this DictionaryObject data)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            Serializer.Serialize(writer, data);
            return builder.ToString();
        }
        
        public static string ToJson(this DictionaryObject[] data)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            Serializer.Serialize(writer, data);
            return builder.ToString();
        }
        
        public static DictionaryObject ParseJsonObject(this string json)
        {
            return JsonConvert.DeserializeObject<DictionaryObject>(json);
        }
        
        public static DictionaryObject[] ParseJsonObjects(this string json)
        {
            return JsonConvert.DeserializeObject<DictionaryObject[]>(json);		
        }
        
        public static DictionaryObject[] ParseFlatObjects(this string json)
        {
            return JsonConvert.DeserializeObject<JArray[]>(json).Single().ToObject<DictionaryObject[]>();		
        }
        
        public static string ParseJsonString(this string json)
        {
            return JsonConvert.DeserializeObject<string>(json);		
        }
        
        public static DictionaryObject GetObject(this DictionaryObject d, string field)
        {		
            return d.Get<DictionaryObject>(field);
        }
	
        public static object GetValue(this DictionaryObject d, string field)
        {
            if (field.Contains("."))
                return d.GetObject(field.Substring(0, field.IndexOf("."))).GetValue(field.Substring(field.IndexOf(".") + 1));
            else
                return d[field];
        }
		
        public static T Get<T>(this DictionaryObject d, string field)
        {
            var value = d.GetValue(field);
            if (value is JContainer j)
                return j.ToObject<T>();
            else
                return (T) value;            
        }
	
        public static IEnumerable<DictionaryObject> GetObjects(this DictionaryObject d, string field)
        {
            return d.Get<DictionaryObject[]>(field);
        }
	
        public static string GetString(this DictionaryObject d, string field) 	
        {
            return d.Get<string>(field);
        }

        public static DateTime GetDate(this DictionaryObject d, string field) 	
        {		
            var timestamp = d.GetLong(field);
            return new DateTime(1970, 1, 1).AddMilliseconds(timestamp);
        }
	
        public static DateTime ToDate(this long t) 
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(t);
        }
	
        public static long GetLong(this DictionaryObject d, string field)
        {		
            return long.Parse(d.GetValue(field).ToString());
        }
        
        public static decimal GetDecimal(this DictionaryObject d, string field, Asset asset)
        {		
            return asset.LongToAmount(long.Parse(d.GetValue(field).ToString()));
        }
	
        public static int GetInt(this DictionaryObject d, string field)
        {		
            return int.Parse(d.GetValue(field).ToString());
        }
        
        public static byte GetByte(this DictionaryObject d, string field)
        {		
            return byte.Parse(d.GetValue(field).ToString());
        }
        
        public static bool GetBool(this DictionaryObject d, string field)
        {		
            return (bool) d.GetValue(field);
        }
    }
}