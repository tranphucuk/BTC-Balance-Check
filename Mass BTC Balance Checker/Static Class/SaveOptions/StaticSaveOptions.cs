using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mass_BTC_Balance_Checker.Static_Class.SaveOptions
{
    public class StaticSaveOptions
    {
        public static readonly string saveFolder = Application.StartupPath + "\\save\\";
        public static readonly string withBalanceFile = saveFolder + "WithBalance.txt";
        public static readonly string emptyBalanceFile = saveFolder + "EmptyBalance.txt";
        public static readonly string errorKeysFile = saveFolder + "ErrorKeys.txt";

        public static void CreatFolderIfNotExist()
        {
            while (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
        }

        public static void CreatFileSaveIfNotExist()
        {
            while (!File.Exists(withBalanceFile))
            {
                File.Create(withBalanceFile).Close();
            }

            while (!File.Exists(emptyBalanceFile))
            {
                File.Create(emptyBalanceFile).Close();
            }

            while (!File.Exists(errorKeysFile))
            {
                File.Create(errorKeysFile).Close();
            }
        }
    }
}
