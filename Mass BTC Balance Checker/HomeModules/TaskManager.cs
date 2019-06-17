using Mass_BTC_Balance_Checker.HomeModules.Events;
using Mass_BTC_Balance_Checker.HomeModules.SSH;
using Mass_BTC_Balance_Checker.Static_Class;
using Mass_BTC_Balance_Checker.Static_Class.HomeModule;
using Mass_BTC_Balance_Checker.Static_Class.SSH;
using OmegaLib.Networks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using thanhps42.HttpClient;

namespace Mass_BTC_Balance_Checker.HomeModules
{
    public class TaskManager
    {
        public Http Http { get; set; }
        public BvSshClient SshClient { get; set; }
        public WithBalanceToSave WithBalance { get; set; }
        public EmptyBalanceToSave EmptyBalance { get; set; }
        public ErrorKeyToSave ErrorKey { get; set; }

        public void ChangeIp(ConcurrentBag<SshDetail> OriginalBag)
        {
            SshAutoControlIfNotExist();
            int port;
            SshDetail currentSsh = null;

            while (true)
            {
                port = NetworkHelper.GetAvailablePort();
                if (OriginalBag.TryTake(out currentSsh))
                {
                    var loginSuccess = SshClient.Login(currentSsh.Host, currentSsh.User, currentSsh.Pass, port, Convert.ToInt32(File.ReadAllText(StaticHomeModule.timeOutSetting)));
                    if (loginSuccess)
                    {
                        break;
                    }
                }
            }
            Http.ProxyType = thanhps42.HttpClient.Enums.ProxyType.Socks5;
            Http.ProxyPort = port;
            Http.SocksVersion = 5;
            Http.SocksHostname = "127.0.0.1";
            Http.SocksPort = port;
        }

        public TaskManager()
        {
            SshClient = new BvSshClient(Application.StartupPath + "\\BvSsh\\BvSsh.exe", Application.StartupPath + "\\BvSsh\\default.bscp", new object());
            Http = new Http();
            WithBalance = new WithBalanceToSave();
            EmptyBalance = new EmptyBalanceToSave();
            ErrorKey = new ErrorKeyToSave();
        }

        private void SshAutoControlIfNotExist()
        {
            var oldControl = Process.GetProcessesByName("autocontrol");
            if (oldControl.Length == 0)
            {
                Process.Start(Application.StartupPath + "\\autocontrol.exe");
            }
        }
    }
}
