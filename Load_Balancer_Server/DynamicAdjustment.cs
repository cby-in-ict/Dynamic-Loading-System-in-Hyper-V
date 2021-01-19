/* This class include memory and CPU resource dynamic adjustment API */

using System;
using System.Collections.Generic;
using System.Text;

namespace Load_Balancer_Server
{
    public class DynamicAdjustment
    {
        public bool isDynamicMem(VirtualMachine VM)
        {
            return VM.performanceSetting.RAM_DynamicMemoryEnabled == true;
        }

        // Adjust Memory related setting API
        public bool AdjustMemorySize(VirtualMachine VM, UInt64 MemorySize)
        {
            bool ret = VM.ModifySettingData("RAM_VirtualQuantity", Convert.ToString(MemorySize));
            return ret;
        }
        public bool AdjustMemoryWeight(VirtualMachine VM, int MemoryWeight)
        {
            bool ret = VM.ModifySettingData("RAM_Weight", Convert.ToString(MemoryWeight));
            return ret;
        }

        // Adjust CPU related setting API
        public bool AdjustCPUCount(VirtualMachine VM, int Count)
        {
            if (VM.IsPowerOn())
                return false;
            
            bool ret = VM.ModifySettingData("CPU_VirtualQuantity", Convert.ToString(Count));
            return ret;
        }
        
        public bool AdjustCPULimit(VirtualMachine VM, int CPULimit)
        {
            bool ret = VM.ModifySettingData("CPU_Limit", Convert.ToString(CPULimit));
            return ret;
        }
        public bool AdjustCPUWeight(VirtualMachine VM, int CPUWeight)
        {
            bool ret = VM.ModifySettingData("CPU_Weight", Convert.ToString(CPUWeight));
            return ret;
        }
        public bool AdjustCPUReservation(VirtualMachine VM, int CPUReservation)
        {
            bool ret = VM.ModifySettingData("CPU_Reservation", Convert.ToString(CPUReservation));
            return ret;
        }
    }
}
