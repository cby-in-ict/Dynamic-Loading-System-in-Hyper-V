#define TEST

//#define Debug
//#define DynamicAdjustTest
//#define GetConfigTest
//#define SystemInfoTest
//#define LoadBalancerTest
//#define DetectorServerTest
//#define VMStateTest
//#define HyperVPerfCounter
//#define CallSetVMStatus
//#define CopyFileTest
#define DockerTest

using System;
using System.Collections.Generic;
using System.Text;
using HyperVWcfTransport;
using System.Management;
using Microsoft.Samples.HyperV.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;

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
            bool ret = LoadBalancer.PreparePowerOnVM(vm, 4096, out bool isRequestMemory);
            ret &= LoadBalancer.ResumePowerOnVM(vm, 1144);

            memSize = memSize - memSize/8;
            dynamicAdjustment.AdjustMemorySize(vm, memSize);
        }
#endif

#if LoadBalancerTest
        public static void LoadBalancerTest()
        {
            string HvAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            Server detectorServer = new Server(HvAddr);
            //

            var task = new Task(() =>
            {
                detectorServer.StartUpServer();
            });
            task.Start();

            ManagementScope scope;
            ManagementObject managementService;

            scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
            managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
            while (Server.mySampleServer.vmPerfDict.Count == 0)
            {
                Thread.Sleep(1000);
            }
            VirtualMachine vm = new VirtualMachine("TestVM", scope, managementService);
            List<VirtualMachine> vmlist = new List<VirtualMachine>();
            vmlist.Add(vm);
            LoadBalancer testLoadBalancer = new LoadBalancer(80.0, 1, 50.0, 3, 1, 1000, 200000);
            testLoadBalancer.setDetectorServer(detectorServer);
            testLoadBalancer.BalanceByTime();
            Console.ReadLine();
        }
#endif
#if DetectorServerTest
        public static void DetectorServerTest()
        {
            string HvAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            Server detectorServer = new Server(HvAddr);
            detectorServer.StartUpServer();
        }
#endif

#if GetConfigTest
#endif

#if VMStateTest
        public static void VMStateTest()
        {
            string MpcVmConfigpath = @"C:\Users\CBY\Documents\代码库\0106\FieldManagerUI\FieldManagerUI\bin\UsefulFile\VMState.json";
            VMState vMState = new VMState(MpcVmConfigpath);
            //VMState.ReceiveMessage();
            vMState.StartMessageReceiver();
            //Console.ReadLine();
        }
#endif
#if HyperVPerfCounter
        public static void HyperVPerfCounter()
        {
            HyperVPerfCounter hyperVPerfCounter = new HyperVPerfCounter();
            while (true) 
            {
                Thread.Sleep(1000);
                VMHvPerfCounterInfo NetVM2HvPerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo("NetVM2");
                Console.WriteLine("当前内存压力为：" + Convert.ToString(NetVM2HvPerfCounterInfo.currentPressure));
            }
            
        }
#endif

#if CallSetVMStatus
        public static bool CallSetVMStatus(string args)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = @".\SetVMStatus.exe";
                info.Arguments = args;
                info.WindowStyle = ProcessWindowStyle.Minimized;
                Process pro = Process.Start(info);
                pro.WaitForExit();
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }
#endif

#if CopyFileTest
        public static void CopyFileTest()
        {
            string MpcVmConfigpath = @"C:\Users\CBY\Documents\代码库\0106\FieldManagerUI\FieldManagerUI\bin\UsefulFile\VMState.json";
            if (!System.IO.File.Exists(MpcVmConfigpath))
            {
                Console.WriteLine("路径" + MpcVmConfigpath + "下，MPC配置文件不存在");
                return;
            }
            VMState vMState = new VMState(MpcVmConfigpath);
            int ret = VMState.LocalVM.CopyFileToGuest("LocalVM", GetConfig.LocalVMProcessInfoPath, @"C:\TEMP\\ProcConfig.json");
            Console.WriteLine("ret is" + Convert.ToString(ret));

        }
#endif

#if WinPerfRecvTest
            ManagementScope scope;
            ManagementObject managementService;

            scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
            managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
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

#if DockerTest
        public static async Task DockerTestAsync()
        {
            DockerLoader dockerLoader = new DockerLoader();
            await dockerLoader.GetContainerListAsync();
            await dockerLoader.GetContainerStatsAsync("9d450203744e", new ContainerStatsParameters { Stream = false});
            Thread.Sleep(1000);
            // GetContainerStats();
            List<DockerLoader.ContainerPerfInfo> ret = dockerLoader.GetContainerPerfInfoListByPS();
            Console.ReadLine();
            foreach (ContainerListResponse containerListResponse in dockerLoader.containerListResponses)
            {
                Console.WriteLine(Convert.ToString(containerListResponse.ID));
                //IProgress<ContainerStatsResponse> statsProgress = dockerLoader.GetContainerStatsAsync(containerListResponse.ID, new ContainerStatsParameters());
                
                Console.WriteLine(Convert.ToString(containerListResponse.Names[0]));
                Console.WriteLine(containerListResponse.State);
            }
            Console.ReadLine();
            DockerLoader.ContainerPerfInfo perfInfo = dockerLoader.GetContainerPerfInfo(id: "9d450203744e");
            Console.WriteLine("内存使用量为：" + Convert.ToString(perfInfo.memUsage) + "KB");
            Console.WriteLine("内存占用率为：" + Convert.ToString(perfInfo.memPercentage));
            Console.WriteLine("CPU占用率为：" + Convert.ToString(perfInfo.cpuPercentage));
        }
#endif

#if TEST
        static async Task Main(string[] args) 
        {
#if CopyFileTest
            CopyFileTest();
            return;
#endif
#if CallSetVMStatus
            bool requestPoweOnRet = CallSetVMStatus("VMStatus LocalVM RequestPowerOn");
#endif
            Console.WriteLine("UnitTest Start");
            if (args.Length > 1)
            {
                string vmName = args[0];
                string vmStatus = args[1];
                VMState.SetVMState(vmName, vmStatus);
                return;
            }
#if SystemInfoTest
            SystemInfoTest();
#endif
#if DetectorServerTest
            // DetectorServerTest();
#endif
#if DynamicAdjustTest
            // DynamicAdjustTest();
#endif
#if LoadBalancerTest
            //LoadBalancerTest();
#endif
#if VMStateTest
            VMStateTest();
#endif
#if HyperVPerfCounter
            HyperVPerfCounter();
#endif
#if DockerTest
            await DockerTestAsync();
#endif

        }
#endif

    }
}
