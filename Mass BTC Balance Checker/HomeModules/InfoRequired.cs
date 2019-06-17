using Mass_BTC_Balance_Checker.Static_Class.SSH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mass_BTC_Balance_Checker.HomeModules
{
    public class InfoRequired
    {
        public List<SshDetail> listSsh { get; set; }
        public List<BtcKeys> keys { get; set; }
        public int Thread { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }
        public string SshFileSelected { get; set; }
    }
}
