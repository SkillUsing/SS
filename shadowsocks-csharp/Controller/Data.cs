using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shadowsocks.Model;

namespace Shadowsocks.Controller
{
    internal enum Label
    {
        H4 = 0,
        P = 1,
    }

    /// <summary>
    /// By Lc
    /// </summary>
    public class Data
    {
        public static int StartIndex { get; } = 1;

        public static int Add { get; } = 12;

        public static int EndIndex { get; } = 27;

        private static readonly List<string> Urls = new List<string>
        {
            "http://www.ishadowsocks.net",
            //"http://i.freevpnss.com"
        };

        public static List<Tuple<string, string, string>> GetHtml_Utf8(List<string> url)
        {
            var wc = new System.Net.WebClient { Encoding = Encoding.UTF8 };
            var rel = new List<Tuple<string, string, string>>();
            foreach (var val in url)
            {
                var str = wc.DownloadString(val);
                var en = Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
                rel.Add(new Tuple<string, string, string>(en, "<h4>", "</h4>"));
            }
            return rel;
        }

        public static string SplitSenior(string str)
        {
            return str.Split(':')[1].Trim();
        }

        public static string[] GetData(Tuple<string, string, string> html)
        {
            var z = html.Item1.Split(new[] { html.Item2, html.Item3 }, StringSplitOptions.RemoveEmptyEntries); //取密码
            return z;
        }

        public static void GetData()
        {
            var rel = GetHtml_Utf8(Urls);
            var data = rel.Select(GetData).ToList();
            var servers = new List<Server>();
            foreach (var val in data)
            {
                for (var i = StartIndex; i < EndIndex; i = i + Add)
                {
                    servers.Add(new Server
                    {
                        server = SplitSenior(val[i]),
                        server_port = int.Parse(SplitSenior(val[i + 2])),
                        method = SplitSenior(val[i + 6]),
                        password = SplitSenior(val[i + 4]),
                        remarks = "",
                        auth = false
                    });
                }
            }
            var s = new ShadowsocksController();
            var c = new Configuration();
            s.SaveServers(servers, c.localPort);
        }
    }
}
