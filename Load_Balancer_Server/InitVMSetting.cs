using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Load_Balancer_Server
{
    // 开启系统后，初始化所有虚拟机的内存和CPU配置
    public class InitVMSetting
    {
        public DynamicAdjustment adjustment = new DynamicAdjustment();
        public void InitAllVM() 
        {
            bool memRet = InitAllVMMem();
            bool cpuRet = InitAllVMCpu();
        }
        public bool InitAllVMMem() 
        {
            bool ret = true;
            if (VMState.VM1 != null)
            {
                ret &= adjustment.AdjustMemorySize(VMState.VM1, VMState.VM1Config.MemorySize);
            }
            if (VMState.VM2 != null)
            {
                ret &= adjustment.AdjustMemorySize(VMState.VM2, VMState.VM2Config.MemorySize);
            }
            if (VMState.VM3 != null)
            {
                ret &= adjustment.AdjustMemorySize(VMState.VM3, VMState.VM3Config.MemorySize);
            }
            return ret;
        }
        public bool InitAllVMCpu()
        {
            bool ret = true;
            if (VMState.VM1 != null)
            {
                if (VMState.VM1.IsPowerOff())
                    ret &= adjustment.AdjustCPUCount(VMState.VM1, VMState.VM1Config.CPUNum);
                else
                    Console.WriteLine("VM1启动状态，初始化阶段无法修改CPU核心数");
                ret &= adjustment.AdjustCPULimit(VMState.VM1, (ulong)(VMState.VM1Config.CPULimt * 1000));
                ret &= adjustment.AdjustCPUReservation(VMState.VM1, 0);
            }
            if (VMState.VM2 != null)
            {
                if (VMState.VM2.IsPowerOff())
                    ret &= adjustment.AdjustCPUCount(VMState.VM2, VMState.VM2Config.CPUNum);
                else
                    Console.WriteLine("VM2启动状态，初始化阶段无法修改CPU核心数");
                ret &= adjustment.AdjustCPULimit(VMState.VM2, (ulong)(VMState.VM2Config.CPULimt * 1000));
                ret &= adjustment.AdjustCPUReservation(VMState.VM2, 0);
            }
            if (VMState.VM3 != null)
            {
                if (VMState.VM3.IsPowerOff())
                    ret &= adjustment.AdjustCPUCount(VMState.VM3, VMState.VM3Config.CPUNum);
                else
                    Console.WriteLine("VM3启动状态，初始化阶段无法修改CPU核心数");
                ret &= adjustment.AdjustCPULimit(VMState.VM3, (ulong)(VMState.VM3Config.CPULimt * 1000));
                ret &= adjustment.AdjustCPUReservation(VMState.VM3, 0);
            }
            return ret;
        }
    }
}
