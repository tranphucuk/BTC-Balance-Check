using DevExpress.XtraBars;
using Mass_BTC_Balance_Checker.HomeModules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace Mass_BTC_Balance_Checker.Static_Class.HomeModule
{
    public class StaticHomeModule
    {
        public static readonly string saveFolder = Application.StartupPath + "\\save\\";
        public static readonly string withBalanceFile = saveFolder + "WithBalance.txt";
        public static readonly string emptyBalanceFile = saveFolder + "EmptyBalance.txt";
        public static readonly string errorKeysFile = saveFolder + "ErrorKeys.txt";
        public static readonly string settingFolder = Application.StartupPath + "\\st\\";
        public static readonly string sshFileSelected = settingFolder + "\\SshSelected.ini";
        public static readonly string timeOutSetting = settingFolder + "\\toSsh.ini";

        public static List<BtcKeys> LoadListKeysFromFile(string[] keys)
        {
            var fileKeys = new List<BtcKeys>();
            foreach (var item in keys)
            {
                var key = new BtcKeys();
                key.Key = item;
                fileKeys.Add(key);
            }
            return fileKeys;
        }

        public static void KillProcessByName(string name)
        {
            var processes = Process.GetProcessesByName(name);
            foreach (var item in processes)
            {
                try
                {
                    item.Kill();
                }
                catch
                {
                }
                
            }
        }

        public static void SshAutoControlIfNostExist()
        {
            var oldControl = Process.GetProcessesByName("autocontrol");
            if (oldControl.Length == 0)
            {
                Process.Start(Application.StartupPath + "\\autocontrol.exe");
            }
        }

        public static void DeleteCreateFileToSaveSettings()
        {
            while (!Directory.Exists(settingFolder))
            {
                Directory.CreateDirectory(settingFolder);
            }
            while (System.IO.File.Exists(timeOutSetting))
            {
                File.Delete(timeOutSetting);
            }
            File.Create(timeOutSetting).Close();
            while (File.Exists(sshFileSelected))
            {
                File.Delete(sshFileSelected);
            }
            File.Create(sshFileSelected).Close();
        }

        public static void SaveSettingsToFile(int timeOut, string sshFile)
        {
            DeleteCreateFileToSaveSettings();
            File.WriteAllText(timeOutSetting, timeOut.ToString());
            File.WriteAllText(sshFileSelected, sshFile);
        }

        public static void ChangeImages(BarStaticItem item)
        {
            try
            {
                item.Caption = " Running...";
                item.ImageOptions.Image = Properties.Resources.loading_1;
                Thread.Sleep(500);

                item.ImageOptions.Image = Properties.Resources.loading_2;
                Thread.Sleep(500);

                item.ImageOptions.Image = Properties.Resources.loading_3;
                Thread.Sleep(500);

                item.ImageOptions.Image = Properties.Resources.loading_4;
                Thread.Sleep(500);
            }
            catch
            {
            }
        }

        #region WRITE BALANCE INFORMATION TO FILE
        public static void WriteWithbalanceToFile(string withBalance)
        {
            try
            {
                FileStream fs = new FileStream(withBalanceFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                var newString = withBalance;
                //Thread.Sleep(2);
                sw.Write(newString + Environment.NewLine);
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static void WriteEmptyBalanceToFile(string emptyBalance)
        {
            try
            {
                FileStream fs = new FileStream(emptyBalanceFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                var newString = emptyBalance;
                //Thread.Sleep(200);
                sw.Write(newString + Environment.NewLine);
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static void WriteErrorKeyToFile(string errorKeys)
        {
            try
            {
                FileStream fs = new FileStream(errorKeysFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                var newString = errorKeys;
                //Thread.Sleep(50);
                sw.Write(newString + Environment.NewLine);
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        } 
        #endregion
    }
}
