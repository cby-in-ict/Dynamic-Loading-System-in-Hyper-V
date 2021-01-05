/* */
using System;
using System.Collections.Generic;
using System.Text;

namespace HyperVWcfTransport
{
    public class VMPerf
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
