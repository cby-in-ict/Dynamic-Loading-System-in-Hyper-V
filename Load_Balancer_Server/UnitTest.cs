#define TEST
#define DynamicAdjustTest
#define GetConfigTest
#define SystemInfoTest
#define LoadBalancerTest
#define DetectorServerTest

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

#if LoadBalancerTest
        public static void LoadBalancerTest()
        {
            ManagementScope scope;
            ManagementObject managementService;

            scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
            managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
            VirtualMachine vm = new VirtualMachine("TestVM", scope, managementService);
            List<VirtualMachine> vmlist = new List<VirtualMachine>();
            vmlist.Add(vm);
            LoadBalancer testLoadBalancer = new LoadBalancer(vmlist, 80.0, 3, 50.0, 3, 3, 10000, 200000);
            testLoadBalancer.BalanceByTime();
        }
#endif
#if DetectorServerTest
        public static void DetectorServerTest()
        {
            string HvAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            DetectorServer detectorServer = new DetectorServer(HvAddr);
            detectorServer.StartUpServer();
        }
#endif

#if GetConfigTest
#endif

#if TEST
        static void Main() 
        {
            Console.WriteLine("UnitTest Start");
#if DetectorServerTest
            DetectorServerTest();
#endif
#if DynamicAdjustTest
            //DynamicAdjustTest();
#endif
#if LoadBalancerTest
            //LoadBalancerTest();
#endif
        }
#endif

    }
    }
