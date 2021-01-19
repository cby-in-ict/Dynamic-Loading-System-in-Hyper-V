#define TEST
#define DynamicAdjustTest
#define GetConfigTest
#define SystemInfoTest

using System;
using System.Collections.Generic;
using System.Text;
using HyperVWcfTransport;
using System.Management;
using Microsoft.Samples.HyperV.Common;

/* Unit test for dynamic loading system in Hyper-V, copyright reserved by chenboyan */
namespace Load_Balancer_Server
{
    class UnitTest
    {
#if SystemInfoTest
        public static void SystemInfoTest()
        {
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
        }
#endif

#if DynamicAdjustTest
        public static void DynamicAdjustTest()
        {
            ManagementScope scope;
            ManagementObject managementService;

            scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
            managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
            VirtualMachine vm = new VirtualMachine("TestVM", scope, managementService);

            UInt64 memSize = vm.performanceSetting.MemoryUsage;
            DynamicAdjustment dynamicAdjustment = new DynamicAdjustment();

            memSize = memSize - memSize/8;
            dynamicAdjustment.AdjustMemorySize(vm, memSize);
        }
#endif

#if GetConfigTest
#endif

#if TEST
        static void Main() 
        {
#if DynamicAdjustTest
            DynamicAdjustTest();
#endif
        }
#endif

    }
}
