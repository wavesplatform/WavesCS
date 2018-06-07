using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web.Script.Serialization;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public static class Api
    {        
        public static event Action<string> DataProcessed;
        
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        

        public static string GetString(string url)
        {
            var json = GetJson(url);
            return (string) Serializer.DeserializeObject(json);
        }                       

        public static DictionaryObject GetObject(string url, params object[] parameters)
        {            
            var json = GetJson(string.Format(url, parameters));            
            return (Dictionary<string, object>) Serializer.DeserializeObject(json);
        }
        
        public static DictionaryObject GetObjectWithHeaders(string url, NameValueCollection headers)
        {
            return GetWithHeaders<DictionaryObject>(url, headers);
        }
        
        public static DictionaryObject[] GetObjectsWithHeaders(string url, NameValueCollection headers)
        {
            return GetWithHeaders<object[]>(url, headers).Cast<DictionaryObject>().ToArray();
        }
        
        public static T GetWithHeaders<T>(string url, NameValueCollection headers)
        {
            var json = GetJson(url, headers);
            return (T) Serializer.DeserializeObject(json);
        }
        
        public static string GetJson(string url, NameValueCollection headers = null)
        {
            OnDataProcessed($"Getting: {url}");
            var client = new WebClient {Encoding = Encoding.UTF8};
            if (headers != null)
                client.Headers.Add(headers);
            var result = client.DownloadString(url);
            OnDataProcessed($"Received: {result}");
            return result;
        }      
        
        public static string Post(string url, DictionaryObject data, NameValueCollection headers = null)
        {
            try
            {
                var client = new WebClient {Encoding = Encoding.UTF8};
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                if (headers != null)
                    client.Headers.Add(headers);            
                var json = Serializer.Serialize(data);
                OnDataProcessed($"Sending to {url} : {json}");
                var response = client.UploadString(url, json);
                OnDataProcessed($"Response: {response}");
                return response;
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                Console.WriteLine(new StreamReader(e.Response.GetResponseStream()).ReadToEnd());                
                throw;
            }
        }

        private static void OnDataProcessed(string obj)
        {
            DataProcessed?.Invoke(obj);
        }
    }
}