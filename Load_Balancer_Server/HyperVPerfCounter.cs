using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace Load_Balancer_Server
{
    public class VMHvPerfCounterInfo 
    {
        public string VMName { set; get; }
        public float averagePressure { set; get; }
        public float currentPressure { set; get; }
        public float availableMemory { set; get; }
        public float guestMemory { set; get; }
        public float availablePercentage { set; get; }
    }
    public class HyperVPerfCounter
    {
        private PerformanceCounter memoryAveragePressure;
        private PerformanceCounter memoryCurrentPressure;
        private PerformanceCounter currentAvailableMemory;
        private PerformanceCounter guestPhycicalMemory;
        public bool SetPerCounter(string VMName)
        {
            try
            {
                memoryCurrentPressure = new PerformanceCounter("Hyper-V Dynamic Memory VM", "Current Pressure", VMName);
                memoryAveragePressure = new PerformanceCounter("Hyper-V Dynamic Memory VM", "Average Pressure", VMName);
                currentAvailableMemory = new PerformanceCounter("Hyper-V Dynamic Memory VM", "Guest Available Memory", VMName);
                guestPhycicalMemory = new PerformanceCounter("Hyper-V Dynamic Memory VM", "Guest Visible Physical Memory", VMName);
                return true;
            }
            catch (Exception exp) 
            {
                return false;
            }
        }
        public HyperVPerfCounter()
        {
            
        }
        public HyperVPerfCounter(string VMName)
        {
            bool ret = SetPerCounter(VMName);
        }
        public VMHvPerfCounterInfo GetVMHyperVPerfInfo(string VMName)
        {
            bool ret = SetPerCounter(VMName);
            if (ret) 
            {
                VMHvPerfCounterInfo vmHvPerfCounterInfo = new VMHvPerfCounterInfo();
                vmHvPerfCounterInfo.VMName = VMName;
                vmHvPerfCounterInfo.averagePressure = memoryAveragePressure.NextValue();
                vmHvPerfCounterInfo.currentPressure = memoryCurrentPressure.NextValue();
                vmHvPerfCounterInfo.availableMemory = currentAvailableMemory.NextValue();
                vmHvPerfCounterInfo.guestMemory = guestPhycicalMemory.NextValue();
                vmHvPerfCounterInfo.availablePercentage = vmHvPerfCounterInfo.availableMemory / vmHvPerfCounterInfo.guestMemory;
                return vmHvPerfCounterInfo;
            }
            else
                return null;
        }
    }
}
