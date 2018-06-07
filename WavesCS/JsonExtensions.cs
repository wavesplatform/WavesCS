using System;
using System.Collections.Generic;
using System.Linq;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace WavesCS
{
    public static class JsonExtensions
    {
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
            return (T) d.GetValue(field);
        }
	
        public static IEnumerable<DictionaryObject> GetObjects(this DictionaryObject d, string field)
        {
            return ((object[]) d.GetValue(field)).Cast<DictionaryObject>();
        }
	
        public static string GetString(this DictionaryObject d, string field) 	
        {
            return d.Get<string>(field);
        }
	
        public static double GetFloat(this DictionaryObject d, string field, int digits) 	
        {		
            return d.GetLong(field) / Math.Pow(10, digits);		
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
    }
}