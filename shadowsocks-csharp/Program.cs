using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

using Shadowsocks.Controller;
using Shadowsocks.Model;
using Shadowsocks.Util;
using Shadowsocks.View;

namespace Shadowsocks
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Utils.ReleaseMemory(true);
            using (Mutex mutex = new Mutex(false, "Global\\Shadowsocks_" + Application.StartupPath.GetHashCode()))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!mutex.WaitOne(0, false))
                {
                    Process[] oldProcesses = Process.GetProcessesByName("Shadowsocks");
                    if (oldProcesses.Length > 0)
                    {
                    }
                    MessageBox.Show(I18N.GetString("Find Shadowsocks icon in your notify tray.") + "\n" +
                        I18N.GetString("If you want to start multiple Shadowsocks, make a copy in another directory."),
                        I18N.GetString("Shadowsocks is already running."));
                    return;
                }
                Directory.SetCurrentDirectory(Application.StartupPath);
#if DEBUG
                Logging.OpenLogFile();

                // truncate privoxy log file while debugging
                string privoxyLogFilename = Utils.GetTempPath("privoxy.log");
                if (File.Exists(privoxyLogFilename))
                    using (new FileStream(privoxyLogFilename, FileMode.Truncate)) { }
#else
                Logging.OpenLogFile();
#endif
                Data.GetData();
                ShadowsocksController controller = new ShadowsocksController();
                MenuViewController viewController = new MenuViewController(controller);
                controller.Start();
                StartTimer();
                Application.Run();
            }
        }


        private static void StartTimer()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Interval = 600000;
            aTimer.Enabled = true;
            GC.KeepAlive(aTimer);
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var x = Data.GetData();
            var controller = new ShadowsocksController();
            var modifiedConfiguration = new Configuration();

            GeteExcellentServer(x, controller, modifiedConfiguration);


            var server = controller.GetCurrentServer();
            controller.SaveServers(modifiedConfiguration.configs, modifiedConfiguration.localPort);
            controller.SelectServerIndex(modifiedConfiguration.configs.IndexOf(server));
        }

        private static Server GetExcellentServer(List<Server> servers, ShadowsocksController controller, Configuration modifiedConfiguration)
        {

            var timeout = 10;
            var options = new PingOptions();
            options.DontFragment = true;
            var reply = new Ping().Send("www.google.com", timeout);
            if (reply != null && reply.Status == IPStatus.Success)
            {
                Console.WriteLine($"==============================================");
                Console.WriteLine($"答复的主机地址：{reply.Address}");
                Console.WriteLine($"往返时间：{reply.RoundtripTime}");
                Console.WriteLine($"生存时间：{reply.Options.Ttl}");
            }
            return servers[0];
        }

    }
}
