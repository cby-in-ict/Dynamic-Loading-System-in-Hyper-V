/* */
using System;
using System.Collections.Generic;
using System.Text;

/* Unit test for dynamic loading system in Hyper-V, copyright reserved by chenboyan */
namespace Load_Balancer_Server
{
    public class VMPerformance
    {
        public struct VMPerfCounter
        {
            float CPU;
            Int64 AvailableMemory;
            float CacheMissRate;
            Int64 PageFaultCount;
            int ProcessQueueLength;
        }

        
        public VMPerfCounter GetVMPerfCounter(string VMName)
        {
            VMPerfCounter currentPerfCount = new VMPerfCounter();
            return currentPerfCount;
        }
    }
}
