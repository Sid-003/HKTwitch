using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using HollowTwitch.Commands;
using UnityEngine;
/// <summary>
/// To Change Client To BiliBili Platform
/// You can change this code "_client = new TwitchClient(Config);" to "_client = new BiliBiliClient(Config);"
/// And the Configuration File you just need to set up two variables
/// Channel is the room id in your channel
/// UserName is your nickname
/// </summary>
namespace HollowTwitch.Clients
{
    public struct Message
    {
        public string user;
        public string time;
        public string text;
        public Message(string nickname, string timeline, string text)
        {
            this.user = nickname;
            this.time = timeline;
            this.text = text;
        }
    }
    internal class BiliBiliClient : IClient
    {
        private readonly List<Message> log = new List<Message>();
        public event Action<string, string> ChatMessageReceived;
        public event Action<string> ClientErrored;
        public event Action<string> RawPayload;
        private static BiliBiliConfig _config;
        private readonly string url = "https://api.live.bilibili.com/xlive/web-room/v1/dM/gethistory";
        private readonly Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"roomid","PUT YOUR ROOM ID HERE" },
                {"csrf_token","" },
                {"csrf","" },
                {"visit_id","" },
            };
        public BiliBiliClient(BiliBiliConfig config)
        {
            if(config.Channel == "-1" || config.Channel == "-2")
            {
                throw new Exception($"BiliBili Channel Error, require number but what you set is \"{config.Channel}\"");
            }

            data["roomid"] = config.Channel;
            RawPayload += ProcessJson;
            _config = config;

        }
        public BiliBiliClient(TwitchConfig twConfig)
        {
            if (twConfig.Channel == "-1" || twConfig.Channel == "-2")
            {
                throw new Exception($"BiliBili Channel Error, require number but what you set is \"{twConfig.Channel}\"");
            }

            data["roomid"] = twConfig.Channel;
            RawPayload += ProcessJson;
            if(twConfig is BiliBiliConfig)
            {
                _config = (BiliBiliConfig)twConfig;
            }
            else
            {
                _config = new BiliBiliConfig
                {
                    Username = twConfig.Username,
                    Channel = twConfig.Channel,
                };
            }

        }
        public void Dispose()
        {
            ClientErrored?.Invoke("BiliBili Client Closed");
        }
        public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }
        public void StartReceive()
        {
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    var message = Post(url, data);
                    RawPayload?.Invoke(message);
                }
                catch (Exception e)
                {
                    ClientErrored?.Invoke("Error occured trying to read stream: " + e);
                }
            
            }
        }

        private bool timeOut(Message m)
        {
            return ( (DateTime.Now - Convert.ToDateTime(m.time)).TotalSeconds > 30 );
        }
        /// <summary>
        /// process the json result which response from BiliBili
        /// </summary>
        /// <param name="json"></param>
        private void ProcessJson(string json)
        {
            if (json != null)
            {
                DanmuJson.Root rt = JsonConvert.DeserializeObject<DanmuJson.Root>(json);
                try
                {
                    var room = rt.data.room;
                    foreach (var r in room)
                    {
                        var m = new Message(r.nickname, r.timeline, r.text);
                        if (!log.Contains(m))
                        {
                            log.Add(m);

                            if (timeOut(m)) // skip command history
                            {
                                continue;
                            }
                            ChatMessageReceived?.Invoke(m.user, m.text);
                        }
                    }
                }
                catch
                {
                    this.ClientErrored.Invoke($"{rt==null} Please Check your Roomid[{data["roomid"]}] \r\n {json}");
                }
                
            }
            if(log.Count > 1000)
            {
                log.RemoveRange(0, 800);
            }
            
        }
        /// <summary>
        /// Request a url in POST method
        /// </summary>
        /// <param name="url">The url you will request</param>
        /// <param name="dic">request argument</param>
        /// <returns></returns>
        public static string Post(string url, Dictionary<string, string> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            #region Add Post Argument
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //Get Response context
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        
    }
}

// this is automatically generate from some tools.
namespace DanmuJson
{

    public class Check_info
    {
        /// <summary>
        /// 
        /// </summary>
        public int ts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ct { get; set; }
    }

    public class AdminItem
    {
        /// <summary>
        /// 游戏：『糖豆人：终极淘汰赛』 平台：Steam、PS4
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int uid { get; set; }
        /// <summary>
        /// 席子酱-Caxiz
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uname_color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string timeline { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int isadmin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int vip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int svip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> medal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> user_level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rank { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int teamid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string rnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user_title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int guard_level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int bubble { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bubble_color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Check_info check_info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int lpl { get; set; }
    }

    public class RoomItem
    {
        /// <summary>
        /// 搓大澡舒服吗
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int uid { get; set; }
        /// <summary>
        /// 守夜冠军007
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uname_color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string timeline { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int isadmin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int vip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int svip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> medal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> user_level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rank { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int teamid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string rnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user_title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int guard_level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int bubble { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bubble_color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Check_info check_info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int lpl { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// 
        /// </summary>
        public List<AdminItem> admin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<RoomItem> room { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Data data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; }
    }



}
