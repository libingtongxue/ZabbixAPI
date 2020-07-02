using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Class
{
    public class ZabbixAPI
    {
        private string IPAddr;
        private string Username;
        private string Password;
        private string ZabbixAPIUrl;
        private string ZabbixAPILoginAuthStr;
        private int Count = 1;
        public ZabbixAPI(string _ipaddr)
        {
            this.IPAddr = _ipaddr;
            this.ZabbixAPIUrl = "http://" + this.IPAddr + "/zabbix/api_jsonrpc.php";
        }
        public bool ZabbixAPILogin(string _username, string _password)
        {
            this.Username = _username;
            this.Password = _password;
            ZabbixLogin zabbixLogin = new ZabbixLogin()
            {
                Username = this.Username,
                Password = this.Password
            };
            ZabbixRequest zabbixRequest = new ZabbixRequest()
            {
                JsonRPC = "2.0",
                Method = "user.login",
                Params = zabbixLogin,
                Id = Count++
            };
            byte[] postBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(zabbixRequest));
            ZabbixResponse zabbixResponse = JsonConvert.DeserializeObject<ZabbixResponse>(Request(postBytes));
            if (!string.IsNullOrEmpty(zabbixResponse.Result.ToString()))
            {
                ZabbixAPILoginAuthStr = zabbixResponse.Result.ToString();
                return true;
            }
            else
            {
                ZabbixAPILoginAuthStr = string.Empty;
                return false;
            }
        }
        public bool ZabbixAPILogout()
        {
            ZabbixRequestAuth zabbixRequest = new ZabbixRequestAuth()
            {
                JsonRPC = "2.0",
                Method = "user.logout",
                Params = Array.Empty<object>(),
                Id = Count++,
                Auth = ZabbixAPILoginAuthStr
            };
            byte[] postBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(zabbixRequest));
            ZabbixResponse zabbixResponse = JsonConvert.DeserializeObject<ZabbixResponse>(Request(postBytes));
            return Boolean.Parse(zabbixResponse.Result.ToString());
        }
        public string ZabbixVersion()
        {
            ZabbixRequest zabbixRequest = new ZabbixRequest()
            {
                JsonRPC = "2.0",
                Method = "apiinfo.version",
                Params = Array.Empty<Object>(),
                Id = Count++
            };
            byte[] postBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(zabbixRequest));
            ZabbixResponse zabbixResponse = JsonConvert.DeserializeObject<ZabbixResponse>(Request(postBytes));
            return zabbixResponse.Result.ToString();
        }
        public string Request(byte[] postBytes)
        {
            string data = "";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ZabbixAPIUrl);
            httpWebRequest.Method = "post";
            httpWebRequest.ContentType = "application/json";
            using (Stream writeStream = httpWebRequest.GetRequestStream())
            {
                writeStream.Write(postBytes, 0, postBytes.Length);
            }
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream receiveStream = httpWebResponse.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        data = readStream.ReadToEnd();
                        readStream.Close();
                    }
                }
                httpWebResponse.Close();
            }
            return data;
        }
    }
    class ZabbixRequest
    {
        [JsonProperty("jsonrpc")]
        public string JsonRPC { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("params")]
        public object Params { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
    }
    class ZabbixResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRPC { get; set; }
        [JsonProperty("result")]
        public object Result { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
    }
    class ZabbixLogin
    {
        [JsonProperty("user")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
    class ZabbixRequestAuth
    {
        [JsonProperty("jsonrpc")]
        public string JsonRPC { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("params")]
        public object Params { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("auth")]
        public string Auth { get; set; }
    }
}
