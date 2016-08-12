using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Shadowsocks.Model;

namespace Shadowsocks.Controller
{
    /// <summary>
    /// By Lc
    /// </summary>
    public class Data
    {
        public int StartIndex { get; } = 1;

        public int Add { get; } = 12;

        public int EndIndex { get; } = 27;

        private readonly List<string> _urls = new List<string>
        {
            "http://www.ishadowsocks.org",
            "http://i.freevpnss.com"
        };

        public List<Tuple<string, string>> GetHtml_Utf8(List<string> url)
        {
            using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
            {
                var result = new List<Tuple<string, string>>();
                foreach (var val in url)
                {
                    try
                    {
                        var htmlStr = wc.DownloadString(val);
                        if (htmlStr.Trim() != "" && val.Contains("ishadowsocks"))
                        {
                            result.Add(new Tuple<string, string>(htmlStr, "h4"));
                        }
                        else if (htmlStr.Trim() != "")
                        {
                            result.Add(new Tuple<string, string>(htmlStr, "p"));
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("下载失败!");
                    }
                }
                return result;
            }
        }

        public string[] GetData(Tuple<string, string> html)
        {
            string[] rel;
            switch (html.Item2)
            {
                case "h4":
                    rel = html.Item1.Split(new[] { "<h4>", "</h4>" }, StringSplitOptions.RemoveEmptyEntries);
                    break;
                case "p":
                    var test = html.Item1.Split(new[] { "<h1>免费SS帐号-每12小时更换一次密码</h1>" }, StringSplitOptions.RemoveEmptyEntries);
                    rel = test.Length > 0 ? test[1].Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries) : null;
                    break;
                default:
                    rel = null;
                    break;
            }
            return rel;
        }

        public List<Server> GetData()
        {
            var rel = GetHtml_Utf8(_urls);

            var data = rel.Select(GetData).Where(relData => relData != null).ToList();

            var servers = new List<Server>();

            var adds = 0;

            var counter = 0;

            foreach (var val in data)
            {
                if (counter != 0)
                {
                    adds = -2;
                }
                for (var i = StartIndex; i < EndIndex; i = i + Add + adds)
                {
                    var server = new Server
                    {
                        server = SplitSenior(val[i]),
                        server_port = int.Parse(SplitSenior(val[i + 2])),
                        method = SplitSenior(val[i + 6]),
                        password = SplitSenior(val[i + 4]),
                        remarks = "",
                    };
                    if (string.IsNullOrWhiteSpace(server.password))
                    {
                        continue;
                    }
                    servers.Add(server);
                }
                counter++;
            }
            return servers;
        }

        public string SplitSenior(string str)
        {
            var rel = "";
            var extract = false;
            foreach (var item in str)
            {
                if (extract)
                {
                    rel += item;
                }
                if (item == ':' || item == '：')
                {
                    extract = true;
                }
            }
            return rel;
        }


        /// <summary>  
        /// 根据主机名（域名）获得主机的IP地址  
        /// </summary>  
        /// <param name="hostName">主机名或域名</param>  
        /// <example> GetIPByDomain("www.google.com");</example>  
        /// <returns>主机的IP地址</returns>  
        public string GetIpByHostName(string hostName)
        {
            hostName = hostName.Trim();
            if (hostName == string.Empty)
                return string.Empty;
            try
            {
                var host = Dns.GetHostEntry(hostName);
                return host.AddressList.GetValue(0).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public bool Ping(string ip)
        {
            var p = new System.Net.NetworkInformation.Ping();
            var options = new System.Net.NetworkInformation.PingOptions
            {
                DontFragment = true
            };
            const string data = "Test Data!";
            var buffer = Encoding.ASCII.GetBytes(data);
            const int timeout = 1000;
            var reply = p.Send(ip, timeout, buffer, options);
            return reply != null && reply.Status == System.Net.NetworkInformation.IPStatus.Success;
        }
    }
}


