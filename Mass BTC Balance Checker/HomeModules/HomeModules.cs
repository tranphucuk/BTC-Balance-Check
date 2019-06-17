using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Mass_BTC_Balance_Checker.Static_Class.SaveOptions;
using Mass_BTC_Balance_Checker.Static_Class;
using System.IO;
using Mass_BTC_Balance_Checker.Static_Class.HomeModule;
using Mass_BTC_Balance_Checker.Static_Class.SSH;
using ZXing;
using System.Threading;

namespace Mass_BTC_Balance_Checker.HomeModules
{
    public partial class HomeModule : DevExpress.XtraEditors.XtraUserControl
    {
        public HomeModule()
        {
            InitializeComponent();
            LoadDefaultSettings();
            CheckForIllegalCrossThreadCalls = false;
            checkAsyn.KeyQueue += CheckAsyn_KeyQueue;
            checkAsyn.KeyChecked += CheckAsyn_KeyChecked;
            checkAsyn.ErrorKey += CheckAsyn_ErrorKey;
            checkAsyn.WithBalance += CheckAsyn_WithBalance;
            checkAsyn.EmptyBalance += CheckAsyn_EmptyBalance;
            checkAsyn.ProxyUsed += CheckAsyn_ProxyUsed;
            checkAsyn.TaskStop += CheckAsyn_TaskStop;
        }

        #region Events Call Back
        private void CheckAsyn_TaskStop(object sender, Events.TaskStopEventArg e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                if (e.ThreadNumber == Convert.ToInt32(seThread.Value))
                {
                    bstUpdateStatus.Caption = e.StopReason + $"{e.ThreadNumber}/{Convert.ToInt32(seThread.Value).ToString()} (DONE).";
                }
                else
                {
                    bstUpdateStatus.Caption = e.StopReason + $"{e.ThreadNumber}/{Convert.ToInt32(seThread.Value).ToString()} ...";
                }
            }));
        }

        private void CheckAsyn_ProxyUsed(object sender, Events.ProxyUsedEventArg e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                teProxyUsed.Text = e.ProxyUsed.ToString();
            }));
        }

        private void CheckAsyn_EmptyBalance(object sender, Events.EmptyBalanceEventArg e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                teEmptyBalance.Text = e.EmptyBalance.ToString();
            }));
        }

        private void CheckAsyn_WithBalance(object sender, Events.WithBalanceEventArg e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                teWithBalance.Text = e.WithBalance.ToString();
            }));
        }

        private void CheckAsyn_ErrorKey(object sender, Events.ErrorKeysEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                teErrorKey.Text = e.ErrorKeys.ToString();
            }));
        }

        private void CheckAsyn_KeyChecked(object sender, Events.KeyCheckedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                teKeyChecked.Text = e.KeyChecked.ToString();
            }));
        }

        private void CheckAsyn_KeyQueue(object sender, Events.KeyQueueEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                teKeyQueue.Text = e.KeyQueue.ToString();
            }));
        }
        #endregion

        public CheckKeyAsync checkAsyn = new CheckKeyAsync();
        public void LoadDefaultSettings()
        {
            StaticSaveOptions.CreatFolderIfNotExist();
            StaticSaveOptions.CreatFileSaveIfNotExist();
            var sshFiles = StaticSsh.LoadFileSshFromFolder();
            leProxy.Properties.DataSource = sshFiles;
            leProxy.EditValue = sshFiles[0];
            var countListSshFromFile = StaticSsh.LoadListSshFromFile(leProxy.EditValue.ToString()).Count;
            teProxyCount.Text = countListSshFromFile.ToString();
        }

        private void leProxy_EditValueChanged(object sender, EventArgs e)
        {
            var ListSshFromFile = StaticSsh.LoadListSshFromFile(leProxy.EditValue.ToString());
            teProxyCount.Text = ListSshFromFile.Count.ToString();

            listSsh = new List<SshDetail>();
            listSsh = ListSshFromFile;
        }

        private void sbLoadKey_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "File Keys|*.txt";
            ofd.Title = "File Keys";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.Cancel) return;

            var files = File.ReadAllLines(ofd.FileName);
            if (files.Length == 0) return;
            var listKeys = StaticHomeModule.LoadListKeysFromFile(files);
            teKeyLoaded.Text = listKeys.Count.ToString();

            keys = new List<BtcKeys>();
            keys = listKeys;
        }

        private List<BtcKeys> keys;
        private List<SshDetail> listSsh;
        //private bool isStopScan;
        private async void sbStart_Click(object sender, EventArgs e)
        {
            if (teKeyLoaded.Text == string.Empty) { XtraMessageBox.Show("Private Keys must not be Empty.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            DisableFunctions();
            StaticHomeModule.SaveSettingsToFile(Convert.ToInt32(seTimeOut.Value), leProxy.EditValue.ToString());
            await checkAsyn.CheckBalanceAsyn(new InfoRequired()
            {
                keys = keys,
                listSsh = listSsh,
                Port = Convert.ToInt32(sePort.Value),
                Thread = Convert.ToInt32(seThread.Value),
                Timeout = Convert.ToInt32(seTimeOut.Value),
                SshFileSelected = leProxy.EditValue.ToString()
            });
            EnableFunctions();
        }

        private void EnableFunctions()
        {
            trmRunTime.Stop();
            bstRunning.Caption = "Stopped...";
            sbStart.Enabled = sbLoadKey.Enabled = true;
            (this.FindForm() as Form1).tslHome.Enabled = true;
            (this.FindForm() as Form1).tslAbout.Enabled = true;
            (this.FindForm() as Form1).tslSettings.Enabled = true;
        }

        private void DisableFunctions()
        {
            trmRunTime.Start();
            sbStart.Enabled = sbLoadKey.Enabled = false;
            (this.FindForm() as Form1).tslHome.Enabled = false;
            (this.FindForm() as Form1).tslAbout.Enabled = false;
            (this.FindForm() as Form1).tslSettings.Enabled = false;
        }

        private void sbStop_Click(object sender, EventArgs e)
        {
            checkAsyn.StopScan();
            lockTaken = true;
        }

        private int ElapsedTime;
        public object o = new object();
        public bool lockTaken = false;
        private void trmRunTime_Tick(object sender, EventArgs e) //StaticHomeModule.ChangeImages(bstRunning);
        {
            new Thread(() =>
            {
                while (true)
                {
                    lock (o) { if (lockTaken) { break; } StaticHomeModule.ChangeImages(bstRunning); }
                }
            })
            { IsBackground = true }.Start();
            ElapsedTime++;
            var timeToDisplay = TimeSpan.FromSeconds(ElapsedTime).ToString("hh\\:mm\\:ss");
            bstTime.Caption = timeToDisplay;
        }
    }
}
