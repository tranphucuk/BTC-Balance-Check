using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mass_BTC_Balance_Checker.HomeModules.Events
{
    public class TaskStopEventArg
    {
        public int ThreadNumber { get; set; }
        public string StopReason { get; set; }
    }
}
