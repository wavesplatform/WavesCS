using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace WavesCS
{
    public static class JsonExtensions
    {
        public static Dictionary<string, object> GetObject(this Dictionary<string, object> d, string field)
        {		
            return d.Get<Dictionary<string, object>>(field);
        }
	
        public static object GetValue(this Dictionary<string, object> d, string field)
        {
            if (field.Contains("."))
                return d.GetObject(field.Substring(0, field.IndexOf("."))).GetValue(field.Substring(field.IndexOf(".") + 1));
            else
                return d[field];
        }
		
        public static T Get<T>(this Dictionary<string, object> d, string field)
        {
            return (T) d.GetValue(field);
        }
	
        public static IEnumerable<Dictionary<string, object>> GetObjects(this Dictionary<string, object> d, string field)
        {
            return ((object[]) d.GetValue(field)).Cast<Dictionary<string, object>>();
        }
	
        public static string GetString(this Dictionary<string, object> d, string field) 	
        {
            return d.Get<string>(field);
        }
	
        public static double GetFloat(this Dictionary<string, object> d, string field, int digits) 	
        {		
            return d.GetLong(field) / Math.Pow(10, digits);		
        }
	
        public static DateTime GetDate(this Dictionary<string, object> d, string field) 	
        {		
            var timestamp = d.GetLong(field);
            return new DateTime(1970, 1, 1).AddMilliseconds(timestamp);
        }
	
        public static DateTime ToDate(this long t) 
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(t);
        }
	
        public static long GetLong(this Dictionary<string, object> d, string field)
        {		
            return long.Parse(d.GetValue(field).ToString());
        }
	
        public static int GetInt(this Dictionary<string, object> d, string field)
        {		
            return int.Parse(d.GetValue(field).ToString());
        }
    }
}