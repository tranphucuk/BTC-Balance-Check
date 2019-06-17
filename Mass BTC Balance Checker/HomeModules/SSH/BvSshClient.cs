using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using thanhps42.BvSsh;

namespace Mass_BTC_Balance_Checker.HomeModules.SSH
{
   public class BvSshClient : BvSshHelper
    {
        private string exePath;
        private string profilePath;
        public BvSshClient(string exe, string profile, object oAutoIt) : base(exe, profile, oAutoIt)
        {
            this.exePath = exe;
            this.profilePath = profile;
        }
    }
}
