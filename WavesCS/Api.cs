using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public static class Api
    {        
        public static event Action<string> DataProcessed;
        
        public static string GetString(string url)
        {
            var json = GetJson(url);
            return json.ParseJsonString();
        }                       

        public static DictionaryObject GetObject(string url, params object[] parameters)
        {            
            var json = GetJson(string.Format(url, parameters));            
            return json.ParseJsonObject();
        }
        
        public static DictionaryObject[] GetObjects(string url, params object[] args)
        {
            var json = GetJson(string.Format(url, args));
            return json.ParseJsonObjects();		
        }
        
        public static DictionaryObject[] GetObjectsWithHeaders(string url, NameValueCollection headers)
        {
            var json = GetJson(url, headers);            
            return json.ParseJsonObjects();
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
                var json = data.ToJson();
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