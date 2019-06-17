using Gma.QrCodeNet.Encoding;
using Mass_BTC_Balance_Checker.HomeModules.Events;
using Mass_BTC_Balance_Checker.HomeModules.SSH;
using Mass_BTC_Balance_Checker.Static_Class;
using Mass_BTC_Balance_Checker.Static_Class.HomeModule;
using Mass_BTC_Balance_Checker.Static_Class.SSH;
using QRCoder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thanhps42.HttpClient;
using ZXing;

namespace Mass_BTC_Balance_Checker.HomeModules
{
    public class CheckKeyAsync
    {
        private List<CancellationTokenSource> cancelSources = new List<CancellationTokenSource>();
        public Dictionary<int, TaskManager> _taskManagers = new Dictionary<int, TaskManager>();
        private void AddTaskManager(InfoRequired info)
        {
            if (_taskManagers.Count > 0) _taskManagers.Clear();
            for (int i = 0; i < info.Thread; i++)
            {
                _taskManagers.Add(i + 1, new TaskManager());
            }
        }
        private TaskManager GetTaskManager(int taskId)
        {
            return _taskManagers[taskId];
        }

        #region Events KEY (QUEUE - CHECKED - ERRROR)
        public event EventHandler<TaskStopEventArg> TaskStop;
        protected virtual void OnTaskStop(TaskStopEventArg e)
        {
            TaskStop?.Invoke(this, e);
        }

        public event EventHandler<ProxyUsedEventArg> ProxyUsed;
        protected virtual void OnProxyUsed(ProxyUsedEventArg e)
        {
            ProxyUsed?.Invoke(this, e);
        }

        public event EventHandler<WithBalanceEventArg> WithBalance;
        protected virtual void OnWithBalance(WithBalanceEventArg e)
        {
            WithBalance?.Invoke(this, e);
        }
        public event EventHandler<EmptyBalanceEventArg> EmptyBalance;
        protected virtual void OnEmptyBalance(EmptyBalanceEventArg e)
        {
            EmptyBalance?.Invoke(this, e);
        }

        public event EventHandler<KeyQueueEventArgs> KeyQueue;
        protected virtual void OnKeyQueue(KeyQueueEventArgs e)
        {
            KeyQueue?.Invoke(this, e);
        }
        public event EventHandler<KeyCheckedEventArgs> KeyChecked;
        protected virtual void OnKeyChecked(KeyCheckedEventArgs e)
        {
            KeyChecked?.Invoke(this, e);
        }
        public event EventHandler<ErrorKeysEventArgs> ErrorKey;
        protected virtual void OnErrorKey(ErrorKeysEventArgs e)
        {
            ErrorKey?.Invoke(this, e);
        }
        #endregion

        private int temSshCount;
        public async Task CheckBalanceAsyn(InfoRequired info)
        {
            StaticHomeModule.KillProcessByName("BvSsh");
            _taskManagers.Clear();
            cancelSources.Clear();

            AddTaskManager(info);
            var keyBag = new ConcurrentBag<BtcKeys>(info.keys);
            var sshBag = RenewSsh(info.SshFileSelected);
            var temKeyBag = keyBag.Count;
            temSshCount = StaticSsh.LoadListSshFromFile(info.SshFileSelected).Count;

            listKeyHaveMoney = new List<WithBalanceToSave>();
            listEmptyBalance = new List<EmptyBalanceToSave>();
            listErrorKey = new List<ErrorKeyToSave>();
            await Task.Run(async () =>
           {
               int taskCancel = 0;
               var tasks = new List<Task>();
               for (int i = 1; i <= info.Thread; i++)
               {
                   var _cancelSource = new CancellationTokenSource();
                   int taskId = i;
                   Thread.Sleep(20);
                   var t = Task.Run(() =>
                   {
                       var temTask = taskId;
                       var manager = GetTaskManager(taskId);
                       try
                       {
                           manager.ChangeIp(sshBag);
                           OnProxyUsed(new ProxyUsedEventArg()
                           {
                               ProxyUsed = temSshCount - sshBag.Count,
                           });
                           while (!keyBag.IsEmpty) // >>>>>>>>>>
                           {
                               _cancelSource.Token.ThrowIfCancellationRequested();

                               OnKeyQueue(new KeyQueueEventArgs()
                               {
                                   KeyQueue = keyBag.Count,
                               });
                               OnKeyChecked(new KeyCheckedEventArgs()
                               {
                                   KeyChecked = temKeyBag - keyBag.Count,
                               });
                               if (keyBag.TryTake(out BtcKeys key))
                               {
                                   lock (o) { GetKeybalanceInfo(key, manager, sshBag, temSshCount); }
                               }
                           }
                       }
                       catch (OperationCanceledException)
                       {
                           taskCancel++;
                           OnTaskStop(new TaskStopEventArg()
                           {
                               ThreadNumber = taskCancel,
                               StopReason = $"Stop Thread: "
                           });
                       }
                   }, _cancelSource.Token);
                   cancelSources.Add(_cancelSource);
                   tasks.Add(t);
               }
               await Task.WhenAll(tasks);
               if (taskCancel == info.Thread) return;
               tasks.Clear();
               cancelSources.Clear();
           });
        }

        public object o = new object();
        public void StopScan()
        {
            if (cancelSources.Count == 0) return;
            foreach (CancellationTokenSource source in cancelSources)
            {
                source.Cancel();
            }
        }
        private List<WithBalanceToSave> listKeyHaveMoney;
        private List<EmptyBalanceToSave> listEmptyBalance;
        private List<ErrorKeyToSave> listErrorKey;
        private void GetKeybalanceInfo(BtcKeys key, TaskManager manager, ConcurrentBag<SshDetail> sshBag, int temSshCount)
        {
            var temKey = string.Empty;
            var http = manager.Http;
            HttpResponse res = null;
            try
            {
                res = http.Get($"https://blockchain.info/address/{key.Key}");
            }
            catch (Exception ex)
            {
                var processes = Process.GetProcessesByName("BvSsh").Length == 0;
                while (res == null)
                {
                    if (processes) return;
                    manager.ChangeIp(sshBag);
                    OnProxyUsed(new ProxyUsedEventArg()
                    {
                        ProxyUsed = temSshCount - sshBag.Count,
                    });
                    res = http.Get($"https://blockchain.info/address/{key.Key}");
                }
            }

            if (res.Text.Contains("Oops"))
            {
                manager.ErrorKey.ErrorKeys = key.Key;
                listErrorKey.Add(manager.ErrorKey);
                OnErrorKey(new ErrorKeysEventArgs()
                {
                    ErrorKeys = listErrorKey.Count,
                });
                temKey = key.Key;
                StaticHomeModule.WriteErrorKeyToFile(temKey);
            }
            else
            {
                var numberTransaction = res.DocumentNode.SelectSingleNode("//*[@id='n_transactions']").InnerText;
                //var totalMoneyReceived = res.DocumentNode.SelectSingleNode("//*[@id='total_received']/font/span").InnerText;//*[@id="final_balance"]/font/span
                var finalBalance = res.DocumentNode.SelectSingleNode("//*[@id='final_balance']/font/span").InnerText.Split(' ')[0];
                if (double.Parse((finalBalance)) != 0)
                {
                    manager.WithBalance.NumberTransaction = numberTransaction;
                    manager.WithBalance.FinalBalance = finalBalance;

                    listKeyHaveMoney.Add(manager.WithBalance);
                    OnWithBalance(new WithBalanceEventArg()
                    {
                        WithBalance = listKeyHaveMoney.Count,
                    });
                    temKey = $"{key.Key}({manager.WithBalance.FinalBalance})";
                    StaticHomeModule.WriteWithbalanceToFile(temKey);
                }
                else
                {
                    listEmptyBalance.Add(manager.EmptyBalance);
                    OnEmptyBalance(new EmptyBalanceEventArg()
                    {
                        EmptyBalance = listEmptyBalance.Count,
                    });
                    temKey = key.Key;
                    StaticHomeModule.WriteEmptyBalanceToFile(temKey);
                }
            }
        }
        private ConcurrentBag<SshDetail> RenewSsh(string countrySelected)
        {
            var _sshes = new ConcurrentBag<SshDetail>();
            var sshItems = StaticSsh.LoadListSshFromFile(countrySelected);
            sshItems = sshItems.OrderBy(s => Guid.NewGuid()).Take(sshItems.Count).ToList();
            foreach (var item in sshItems)
            {
                _sshes.Add(item);
            }

            return _sshes;
        }
    }
}
