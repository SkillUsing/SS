﻿using System;
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
                Application.Run();
            }
        }
    }
}
