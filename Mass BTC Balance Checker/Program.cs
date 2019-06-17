using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using thanhps42.Common;
using FoxLearn.License;

namespace Mass_BTC_Balance_Checker
{
    static class Program
    {
        public static string licensePath => Application.StartupPath + "\\license.eth";
        public static string passwordToEncryptKey = "LtiA@Tju(hkw4#U2oi2ty3t2G>WYLWa$^J#$31ScDtiAXcH48PLhS%";
        private static string _computerId;
        public static string ComputerId
        {
            get
            {
                return _computerId;
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _computerId = ComputerInfo.GetComputerId();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            BonusSkins.Register();
            SkinManager.EnableFormSkins();
            Application.Run(new Form1());
        }
    }
}
