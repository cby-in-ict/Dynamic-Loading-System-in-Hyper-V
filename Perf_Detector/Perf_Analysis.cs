using System;
using System.Collections.Generic;
using System.Text;

/* 
 * This class analyse performance in the VM, when detect shortage in Mem and CPU resource,
 * There will be a signal when detect problem, then call the host.
 */
namespace Perf_Detector
{
    public class Perf_Analysis
    {
        public bool isAlarm = false;
        public float CPUUsage = 0;
        public UInt64 MemoryUsage = 0;
        public int ProcessorQueueLength = 0;
        public float CacheMissRate = 0;
        public UInt64 PageFaultCount = 0;
        public MemSpoofer MemSpoofer = new MemSpoofer();
        public CpuSpoofer CpuSpoofer = new CpuSpoofer();
        public ProcessInfo ProcessInfo = new ProcessInfo();

        public bool RaiseAlarm()
        {

            return false;
        }
    }
}
