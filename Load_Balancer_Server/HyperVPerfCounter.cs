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

    }
    public class HyperVPerfCounter
    {
        private PerformanceCounter memoryAveragePressure;
        private PerformanceCounter memoryCurrentPressure;
        public bool SetPerCounter(string VMName)
        {
            try
            {
                if (VMName == "LocalVM")
                {
                    if (VMState.LocalVM == null)
                        return false;
                    if (!VMState.LocalVM.IsPowerOn())
                    {
                        return false;
                    }    
                }
                else if (VMName == "NetVM1")
                {
                    if (VMState.NetVM1 == null)
                        return false;
                    if (!VMState.NetVM1.IsPowerOn())
                    {
                        return false;
                    }
                }
                else if (VMName == "NetVM2")
                {
                    if (VMState.NetVM2 == null)
                        return false;
                    if (!VMState.NetVM2.IsPowerOn())
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("虚拟机名：" + VMName + "，非本地域、互联网域中的任意一种");
                    return false;
                }
                memoryCurrentPressure = new PerformanceCounter("Hyper-V Dynamic Memory VM", "Current Pressure", VMName);
                memoryAveragePressure = new PerformanceCounter("Hyper-V Dynamic Memory VM", "Average Pressure", VMName);
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
            VMHvPerfCounterInfo vmHvPerfCounterInfo = new VMHvPerfCounterInfo();
            vmHvPerfCounterInfo.VMName = VMName;
            vmHvPerfCounterInfo.averagePressure = memoryAveragePressure.NextValue();
            vmHvPerfCounterInfo.currentPressure = memoryCurrentPressure.NextValue();
            return vmHvPerfCounterInfo;
        }
    }
}
