using Mass_BTC_Balance_Checker.Static_Class.SSH;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mass_BTC_Balance_Checker.Static_Class
{
    public class StaticSsh
    {
        public static readonly string folderSsh = Application.StartupPath + "\\SSHFolder\\";

        public static List<string> LoadFileSshFromFolder()
        {
            var listFiles = new List<string>();
            var dir = new DirectoryInfo(folderSsh);
            var files = dir.GetFiles("*.txt");
            if (files.Length == 0) listFiles = null;
            foreach (var item in files)
            {
                var filename = item.Name;
                listFiles.Add(filename);
            }
            return listFiles;
        }

        public static void SaveSshToFile(string sshName, List<SshDetail> listSshImport)
        {
            while (File.Exists(folderSsh + sshName))
            {
                File.Delete(folderSsh + sshName);
            }
            File.Create(folderSsh + sshName).Close();
            StringBuilder builder = new StringBuilder();
            foreach (var item in listSshImport)
            {
                builder.AppendLine($"{item.Host}|{item.User}|{item.Pass}");
            }
            File.WriteAllText(folderSsh + sshName, builder.ToString());
        }

        public static List<SshDetail> LoadListSshFromFile(string fileName)
        {
            var lines = File.ReadAllLines(folderSsh + fileName);
            var sshList = new List<SshDetail>();
            for (int i = 0; i < lines.Length; i++)
            {
                var ssh = new SshDetail();
                var split = lines[i].Split('|');
                if (split.Length < 3) continue;

                ssh.Id = i;
                ssh.Host = split[0];
                ssh.User = split[1];
                ssh.Pass = split[2];
                sshList.Add(ssh);
            }
            return sshList;
        }

        public static int GetIndexOfSshFile(string fileName)
        {
            var sshFolder = new DirectoryInfo(folderSsh);
            var files = sshFolder.GetFiles("*.txt");
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.Contains(fileName)) return i;
            }
            return 0;
        }

        #region CREATE - DELETE SSH FILE 
        public static bool CreatNewFileSsh(string fileName)
        {
            try
            {
                if (Directory.Exists(folderSsh))
                {
                    File.Create(folderSsh + $"{fileName}.txt").Close();
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public static bool DeleteFileSsh(string fileName)
        {
            try
            {
                if (Directory.Exists(folderSsh))
                {
                    File.Delete(folderSsh + fileName);
                    return true;
                }
            }
            catch
            {
            }
            return false;
        } 
        #endregion
    }
}
