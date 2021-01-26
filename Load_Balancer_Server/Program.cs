/*                                ChenBoYan chenboyan@ict.ac.cn
 * This is a Dynamic Balancer for Hyper-V VM, It can ensures a VM has enough free mem to startup,
 * and as the load increase, this system can detect performance shortage and dynamicly adjust the 
 * resource of a VM. for more information, Please refer to Readme.md.
 *                       Copyright preserved by ChenBoYan, chenboyan@ict.ac.cn 
*/
#define TEST
using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Samples.HyperV.Common;
using Perf_Transfer;


namespace Load_Balancer_Server
{
    class Program
    {
        static void Main1(string[] args)
        {
            string MpcVmConfigpath = @"";
            VMState vMState = new VMState(MpcVmConfigpath);

            string HvAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            DetectorServer detectorServer = new DetectorServer(HvAddr);
            detectorServer.StartUpServer();
            //VMPerf currentPerfTransfer = DetectorServer.currentPerfTransfer;
            //Console.WriteLine("可用内存大小为：" + currentPerfTransfer.MEMAvailable);


#if TEST
            ManagementScope scope;
            ManagementObject managementService;

            scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
            managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
            VirtualMachine vm = new VirtualMachine("TestVM", scope, managementService);
            PerformanceSetting performanceSetting = vm.GetPerformanceSetting();
            Console.WriteLine(Convert.ToString(performanceSetting.ProcessorLoad));
            if (performanceSetting.ProcessorLoadHistory != null)
            {
                foreach (ushort it in performanceSetting.ProcessorLoadHistory)
                {
                    Console.WriteLine("历史信息，CPU占用率：" + Convert.ToString(it));
                }
            }

            SystemInfo systemInfo = new SystemInfo();
            Int64 AvailableMemory = systemInfo.MemoryAvailable;
            Int64 PhysicalMemory = systemInfo.PhysicalMemory;
            AvailableMemory = AvailableMemory / (1024 * 1024);
            PhysicalMemory = PhysicalMemory / (1024 * 1024);
            int ProcessorCount = systemInfo.ProcessorCount;
            float load = systemInfo.CpuLoad;

            Console.WriteLine("CPU总个数：" + ProcessorCount);
            Console.WriteLine("CPU负载率：" + load);
            Console.WriteLine("总物理内存：" + PhysicalMemory + "MB");
            Console.WriteLine("当前可用内存：" + AvailableMemory + "MB");
        #endif
        }
    }
}
