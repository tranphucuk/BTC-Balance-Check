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
using Mass_BTC_Balance_Checker.Static_Class;
using System.IO;
using Mass_BTC_Balance_Checker.Static_Class.SSH;
using Mass_BTC_Balance_Checker.Static_Class.SaveOptions;
using System.Diagnostics;

namespace Mass_BTC_Balance_Checker.SettingsModules
{
    public partial class SettingModules : DevExpress.XtraEditors.XtraUserControl
    {
        public SettingModules()
        {
            InitializeComponent();
            LoadFileSshToLookUpEdit(leSshFiles);
            gridView1.Columns["Host"].Width = 30;
            gridView1.Columns["User"].Width = 15;
            gridView1.Columns["Pass"].Width = 15;
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            switch (e.Page.Text.ToString())
            {
                case "Proxy Settings":
                    LoadFileSshToLookUpEdit(leSshFiles);
                    break;
                case "Save Options":
                    CheckExistListSave();
                    break;
                default:
                    break;
            }
        }

        #region SAVE OPTIONS TABPAGE
        private void CheckExistListSave()
        {
            StaticSaveOptions.CreatFolderIfNotExist();
            StaticSaveOptions.CreatFileSaveIfNotExist();
            teListWithBalance.Text = StaticSaveOptions.withBalanceFile;
            teListEmptyBalance.Text = StaticSaveOptions.emptyBalanceFile;
            teListErrorKeys.Text = StaticSaveOptions.errorKeysFile;
        }

        private void lblErrorKeys_Click(object sender, EventArgs e)
        {
            Process.Start(StaticSaveOptions.errorKeysFile);
        }

        private void lblEmptyBalance_Click(object sender, EventArgs e)
        {
            Process.Start(StaticSaveOptions.emptyBalanceFile);
        }

        private void lblWithBalance_Click(object sender, EventArgs e)
        {
            Process.Start(StaticSaveOptions.withBalanceFile);
        } 
        #endregion

        #region DEFAULT SETTINGS MODULE SSH
        private void LoadSshToGridControl(string sshName)
        {
            var listSsh = StaticSsh.LoadListSshFromFile(sshName);
            gridControl1.DataSource = listSsh;
            gridControl1.RefreshDataSource();
            gridView1.Columns["Id"].Visible = false;
        }

        private void LoadFileSshToLookUpEdit(LookUpEdit le)
        {
            var sshFiles = StaticSsh.LoadFileSshFromFolder();
            le.Properties.DataSource = sshFiles;
            le.EditValue = sshFiles[index];
        }

        private int index;
        private void leSshFiles_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            index = StaticSsh.GetIndexOfSshFile(e.NewValue.ToString());
            LoadSshToGridControl(e.NewValue.ToString());
            lblTotalSsh.Text = $"Total : {(gridControl1.DataSource as List<SshDetail>).Count} SSH";
        } 
        #endregion

        #region CREATE - DELETE SSH File(FlyoutPanel)
        private void lblAddNewSsh_Click(object sender, EventArgs e)
        {
            flyoutPanel1.ShowBeakForm();
            LoadFileSshToLookUpEdit(leLoadSshFile);
        }

        private void lblDeleteFileSsh_Click(object sender, EventArgs e)
        {
            var deleteConfirm = XtraMessageBox.Show($"Delete File '{leLoadSshFile.EditValue.ToString()}' ?", "Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (deleteConfirm == DialogResult.Cancel) return;
            var isDeletedSuccess = StaticSsh.DeleteFileSsh(leLoadSshFile.EditValue.ToString());
            if (isDeletedSuccess == false)
            {
                XtraMessageBox.Show("Delete Failed. Try Again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LoadFileSshToLookUpEdit(leLoadSshFile);
            LoadFileSshToLookUpEdit(leSshFiles);
        }

        private void sbSave_Click(object sender, EventArgs e)
        {
            if (teNewSshFile.Text == string.Empty)
            {
                XtraMessageBox.Show("Name must have value.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var isCreateSuccess = StaticSsh.CreatNewFileSsh(teNewSshFile.Text);
            if (isCreateSuccess == false) XtraMessageBox.Show("Creat Failed. Try again.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            LoadFileSshToLookUpEdit(leSshFiles);
            lblTotalSsh.Text = $"Total : {(gridControl1.DataSource as List<SshDetail>).Count} SSH";
            flyoutPanel1.HideBeakForm(false);
        }

        private void sbCancel_Click(object sender, EventArgs e)
        {
            flyoutPanel1.HideBeakForm(false);
        } 
        #endregion

        #region ADD - REMOVE SSH
        private void sbLoadSsh_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "SSH File|*.txt";
            ofd.Title = "SSH File";
            if (ofd.ShowDialog() == DialogResult.Cancel) return;
            var files = File.ReadAllLines(ofd.FileName);
            if (files.Length == 0) return;
            var listSsh = new List<SshDetail>();
            foreach (var item in files)
            {
                var ssh = new SshDetail();
                var split = item.Split('|');
                if (split.Length < 3) continue;
                ssh.Host = split[0];
                ssh.User = split[1];
                ssh.Pass = split[2];
                listSsh.Add(ssh);
            }
            var sshListInGridControl = gridControl1.DataSource as List<SshDetail>;
            foreach (var item in sshListInGridControl)
            {
                listSsh.Add(item);
            }
            StaticSsh.SaveSshToFile(leSshFiles.Text, listSsh);
            LoadSshToGridControl(leSshFiles.Text);
            lblTotalSsh.Text = $"Total : {(gridControl1.DataSource as List<SshDetail>).Count} SSH";
        }

        private void sbRemoveSsh_Click(object sender, EventArgs e)
        {
            var countrySelected = leSshFiles.EditValue.ToString();
            var selectedItems = gridView1.GetSelectedItems<SshDetail>()?.ToList();
            var listSsh = gridControl1.DataSource as List<SshDetail>;
            var remoConfirm = XtraMessageBox.Show($"Delete {selectedItems.Count} SSH ?", "Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (remoConfirm == DialogResult.Cancel) return;
            foreach (var item in selectedItems)
            {
                listSsh.Remove(item);
            }
            StaticSsh.SaveSshToFile(countrySelected, listSsh);
            LoadSshToGridControl(countrySelected);
            lblTotalSsh.Text = $"Total : {(gridControl1.DataSource as List<SshDetail>).Count} SSH";
        }
        #endregion
    }
}
