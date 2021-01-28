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

        // Add Memory according to baseline, the baseline is the user set memory,ie:2048MB, MemorySize is current Memory Size,
        // rank 1:MemorySize + 1/8 * baseline. rank 2:MemorySize + 1/4 * baseline
        public bool AppendVMMemory(VirtualMachine VM, UInt64 MemorySizeBaseLine, UInt64 MemorySize, int rank)
        {
            if (rank == 1)
            {
                UInt64 appendSize = MemorySizeBaseLine / 8;
                bool ret = VM.ModifySettingData("RAM_VirtualQuantity", Convert.ToString(MemorySize + appendSize));
                return ret;
            }
            else if (rank == 2)
            {
                UInt64 appendSize = MemorySizeBaseLine / 4;
                bool ret = VM.ModifySettingData("RAM_VirtualQuantity", Convert.ToString(MemorySize + appendSize));
                return ret;
            }
            else
                return false;
        }
        
        // Recycle Memory according to baseline, the baseline is the user set memory,ie:2048MB, MemorySize is current Memory Size,
        // rank 1:MemorySize - 1/8 * baseline. rank 2:MemorySize - 1/4 * baseline
        public bool RecycleVMMemory(VirtualMachine VM, UInt64 MemorySizeBaseLine, UInt64 MemorySize, int rank)
        {
            if (rank == 1)
            {
                UInt64 recycleSize = MemorySizeBaseLine / 8;
                bool ret = VM.ModifySettingData("RAM_VirtualQuantity", Convert.ToString(MemorySize - recycleSize));
                return ret;
            }
            else if (rank == 2)
            {
                UInt64 recycleSize = MemorySizeBaseLine / 4;
                bool ret = VM.ModifySettingData("RAM_VirtualQuantity", Convert.ToString(MemorySize - recycleSize));
                return ret;
            }
            else
                return false;
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
        
        public bool AdjustCPULimit(VirtualMachine VM, UInt64 CPULimit)
        {
            bool ret = VM.ModifySettingData("CPU_Limit", Convert.ToString(CPULimit));
            return ret;
        }
        public bool AdjustCPUWeight(VirtualMachine VM, int CPUWeight)
        {
            bool ret = VM.ModifySettingData("CPU_Weight", Convert.ToString(CPUWeight));
            return ret;
        }
        public bool AdjustCPUReservation(VirtualMachine VM, UInt64 CPUReservation)
        {
            bool ret = VM.ModifySettingData("CPU_Reservation", Convert.ToString(CPUReservation));
            return ret;
        }
    }
}
