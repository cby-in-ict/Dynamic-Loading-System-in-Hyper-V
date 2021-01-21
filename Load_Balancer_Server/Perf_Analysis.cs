using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using Perf_Transfer;

/* 
 * This class analyse performance in the VM, when detect shortage in Mem and CPU resource,
 * There will be a signal when detect problem, then call the host.
 */
namespace Load_Balancer_Server
{
    public class VMState 
    { 
        public string vmName { get; set; }
        public VMPerf vmPerf { get; set; }

    }
}
