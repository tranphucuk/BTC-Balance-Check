using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Mass_BTC_Balance_Checker.AboutModules;
using Mass_BTC_Balance_Checker.HomeModules;
using Mass_BTC_Balance_Checker.LicenseModule;
using Mass_BTC_Balance_Checker.SettingsModules;
using Mass_BTC_Balance_Checker.Static_Class;
using Mass_BTC_Balance_Checker.Static_Class.HomeModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mass_BTC_Balance_Checker
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        public Form1()
        {
            InitializeComponent();
            //CheckLicense();
        }

        private void CheckLicense()
        {
            var license = LicenseInfoController.Instance.GetLicense();
            switch (license.Type)
            {
                case LicenseType.Invalid:
                    {
                        var frm = new License_Checking() { Text = "Invalid Key" };
                        frm.ShowDialog();
                        this.Close();
                        break;
                    }
                case LicenseType.Valid:
                    break;
                case LicenseType.Expired:
                    {
                        var frm = new License_Checking() { Text = "Key has Expired" };
                        frm.ShowDialog();
                        this.Close();
                        break;
                    }
                default:
                    break;
            }
        }

        private void ChangeModuleTo<T>() where T : XtraUserControl
        {
            SplashScreenManager.ShowForm(FindForm(), typeof(WaitForm1), true, true, false);
            panelControl1.Controls.Clear();
            var newModule = Activator.CreateInstance<T>();
            newModule.Dock = DockStyle.Fill;
            panelControl1.Controls.Add(newModule);
            SplashScreenManager.CloseForm();
        }

        private void tslHome_Click(object sender, EventArgs e)
        {
            ChangeModuleTo<HomeModule>();
        }

        private void tslSettings_Click(object sender, EventArgs e)
        {
            ChangeModuleTo<SettingModules>();
        }

        private void tslAbout_Click(object sender, EventArgs e)
        {
            AboutModule frmAbout = new AboutModule();
            frmAbout.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var quitConfirm = XtraMessageBox.Show("Quit ??", "Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (quitConfirm == DialogResult.Cancel) e.Cancel = true;
            else { StaticHomeModule.KillProcessByName("BvSsh"); }
        }
    }
}
