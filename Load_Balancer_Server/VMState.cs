using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Samples.HyperV.Common;
using Perf_Transfer;

/* 
 * This class analyse performance in the VM, when detect shortage in Mem and CPU resource,
 * There will be a signal when detect problem, then call the host.
 */
namespace Load_Balancer_Server
{
    public class VMState 
    {
        public static bool isDockerInstalled = false;
        public static bool isDockerVMPowerOn = false;
        public static string currentUsedVM = "GetOff";
        public static VirtualMachine VM1;
        public static VMHvPerfCounterInfo VM1PerfCounterInfo = new VMHvPerfCounterInfo();
        public static VMConfig VM1Config;
        public static VirtualMachine VM2;
        public static VMHvPerfCounterInfo VM2PerfCounterInfo = new VMHvPerfCounterInfo();
        public static VMConfig VM2Config;
        public static VirtualMachine VM3;
        public static VMHvPerfCounterInfo VM3PerfCounterInfo = new VMHvPerfCounterInfo();
        public static VMConfig VM3Config;
        public static VirtualMachine DockerDesktopVM = null;
        public static VMHvPerfCounterInfo DockerVMPerfCounterInfo = new VMHvPerfCounterInfo();
        public static VMConfig DockerVMConfig;
        public VMState()
        {
            ManagementScope scope;
            ManagementObject managementService;

            scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
            managementService = WmiUtilities.GetVirtualMachineManagementService(scope);

            GetConfig configParser = new GetConfig();
            VM1Config = configParser.GetVMConfig("VM1");
            if (VM1Config != null)
                VM1 = new VirtualMachine(VM1Config.VMName, scope, managementService);
            VM2Config = configParser.GetVMConfig("VM2");
            if (VM2Config != null)
                VM2 = new VirtualMachine(VM2Config.VMName, scope, managementService);
            VM3Config = configParser.GetVMConfig("VM3");
            if (VM3Config != null)
                VM3 = new VirtualMachine(VM3Config.VMName, scope, managementService);
            DockerVMConfig = configParser.GetVMConfig("Docker");
        }
        public VMState(string MpcVMConfigPath)
        {
            GetConfig configParser = new GetConfig();
            VM1Config = configParser.GetVMConfig("VM1");
            VM2Config = configParser.GetVMConfig("VM2");
            VM3Config = configParser.GetVMConfig("VM3");

            configParser.currentMpcVMInfo = configParser.GetMpcVMInfo(MpcVMConfigPath, "LocalVM");
            if (configParser.currentMpcVMInfo.Installed == true)
            {
                ManagementScope scope;
                ManagementObject managementService;

                scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                VM1 = new VirtualMachine("LocalVM", scope, managementService);
            }
            configParser.currentMpcVMInfo = configParser.GetMpcVMInfo(MpcVMConfigPath, "NetVM1");
            if (configParser.currentMpcVMInfo.Installed == true)
            {
                ManagementScope scope;
                ManagementObject managementService;

                scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                VM2 = new VirtualMachine("NetVM1", scope, managementService);
            }
            configParser.currentMpcVMInfo = configParser.GetMpcVMInfo(MpcVMConfigPath, "NetVM2");
            if (configParser.currentMpcVMInfo.Installed == true)
            {
                ManagementScope scope;
                ManagementObject managementService;

                scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                VM3 = new VirtualMachine("NetVM2", scope, managementService);
            }

        }

        public static void SetVMState(string VMName, string State)
        {
            if (VMName.Contains("LocalVM"))
            {
                if (VM1 == null && State != "Install")
                {
                    Console.WriteLine("LocalVM 未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        Console.WriteLine("LocalVM 转换为关机状态");
                        VM1.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        VM1.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        bool requestPowerOnRet = LoadBalancer.PreparePowerOnVM(VM1, VM1Config.MemorySize, out bool isRequestMemory);
                        if (isRequestMemory)
                        {
                            // 尝试回收其他虚拟机内存
                        }
                        VM1.PowerOn();
                        Console.WriteLine("LocalVM 尝试开机成功");
                        break;
                    case "PowerOn":
                        VM1.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        bool resumePowerOnRet = LoadBalancer.ResumePowerOnVM(VM1, VM1Config.MemorySize);
                       int copyFileRet = VM1.CopyFileToGuest("LocalVM", GetConfig.LocalVMProcessInfoPath, @"C:\TEMP\\ProcConfig.json");
                        if (copyFileRet == 0)
                            Console.WriteLine("拷贝本地域进程控制配置文件成功！");
                        //if (copyFileRet == 0)
                        //    Console.WriteLine("进程控制信息文件拷贝到本地域");
                        Console.WriteLine("LocalVM 转换为开机状态");
                        break;
                    case "UnInstall":
                        VM1 = null;
                        break;
                    case "Install":
                        ManagementScope scope;
                        ManagementObject managementService;

                        scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                        managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                        VM1 = new VirtualMachine("LocalVM", scope, managementService);
                        break;
                    default:
                        break;
                }
                return;
            }
            else if (VMName.Contains("NetVM1"))
            {
                if (VM2 == null && State != "Install")
                {
                    Console.WriteLine("NetVM1 未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        VM2.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        Console.WriteLine("NetVM1 转换为关机状态");
                        break;
                    case "RequestPowerOn":
                        VM2.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        bool requestPowerOnRet = LoadBalancer.PreparePowerOnVM(VM2, VM2Config.MemorySize, out bool isRequestMemory);
                        if (isRequestMemory)
                        {
                            // 尝试回收其他虚拟机内存
                        }
                        VM2.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        VM2.PowerOn();
                        Console.WriteLine("NetVM1 尝试开机成功");
                        break;
                    case "PowerOn":
                        VM2.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        bool resumePowerOnRet = LoadBalancer.ResumePowerOnVM(VM2, VM2Config.MemorySize);
                        int copyFileRet = VM2.CopyFileToGuest("NetVM1", GetConfig.NetVM1ProcessInfoPath, @"C:\TEMP\ProcConfig.json");
                        if (copyFileRet == 0)
                            Console.WriteLine("拷贝进程控制配置文件到互联网域1成功！");
                        Console.WriteLine("NetVM1 转换为开机状态");
                        break;
                    case "UnInstall":
                        VM2 = null;
                        break;
                    case "Install":
                        ManagementScope scope;
                        ManagementObject managementService;

                        scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                        managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                        VM2 = new VirtualMachine("NetVM1", scope, managementService);
                        break;
                    default:
                        break;
                }
                return;
            }
            else if (VMName.Contains("NetVM2"))
            {
                if (VM3 == null && State != "Install")
                {
                    Console.WriteLine("NetVM2 未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        VM3.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        Console.WriteLine("NetVM2 转换为关机状态");
                        break;
                    case "RequestPowerOn":
                        VM3.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        bool requestPowerOnRet = LoadBalancer.PreparePowerOnVM(VM3, VM3Config.MemorySize, out bool isRequestMemory);
                        if (isRequestMemory)
                        {
                            // 尝试回收其他虚拟机内存
                        }
                        VM3.PowerOn();
                        Console.WriteLine("NetVM2 尝试开机成功");
                        break;
                    case "PowerOn":
                        VM3.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        bool resumePowerOnRet = LoadBalancer.ResumePowerOnVM(VM3, VM3Config.MemorySize);
                        //int copyFileRet = NetVM2.CopyFileToGuest("NetVM2", GetConfig.NetVM2ProcessInfoPath, @"C:\TEMP\ProcConfig.json");
                        //if (copyFileRet == 0)
                        //    Console.WriteLine("拷贝互联网域2进程控制配置文件成功！");
                        Console.WriteLine("NetVM2 转换为开机状态");
                        break;
                    case "UnInstall":
                        VM3 = null;
                        break;
                    case "Install":
                        ManagementScope scope;
                        ManagementObject managementService;

                        scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                        managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                        VM3 = new VirtualMachine("NetVM2", scope, managementService);
                        break;
                    default:
                        break;
                }
                return;
            }
            else
                return;
        }

        public static void DetectVMState(object sender, ElapsedEventArgs e)
        {
            if (DockerDesktopVM == null)
            {
                try
                {
                    ManagementScope scope;
                    ManagementObject managementService;

                    scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                    managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                    DockerDesktopVM = new VirtualMachine("DockerDesktopVM", scope, managementService);
                    isDockerInstalled = true;
                }
                catch
                {
                    DockerDesktopVM = null;
                    isDockerInstalled = false;
                }
            }
            else
            {
                isDockerVMPowerOn = DockerDesktopVM.IsPowerOn();
            }
        }

        public void StartDetectVMState(int detectTimeGap)
        {
            System.Timers.Timer RUtimer = new System.Timers.Timer(detectTimeGap);    // 参数单位为ms
            RUtimer.Elapsed += DetectVMState;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }

        public static void ReceiveMessage()
        {
            MemoryMapping memoryMapping = new MemoryMapping("vmStatus");
            while (true)
            {
                try
                {
                    string recStr = memoryMapping.ReadString();
                    recStr = recStr.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("\f", "").Replace("\v", "");
                    if (recStr.Split('#')[0].Contains("GetIn"))
                    {
                        currentUsedVM = recStr.Split('#')[1];
                    }
                    else if (recStr.Split('#')[0].Contains("GetOff"))
                    {
                        currentUsedVM = "GetOff";
                    }
                    else
                    {
                        string vmName = recStr.Split('#')[0];
                        string vmStatus = recStr.Split('#')[1];
                        SetVMState(vmName, vmStatus);
                        //memoryMapping.m_Received.Release();
                    }
                    memoryMapping.ReleaseReceiveSemaphore();
                }
                catch (Exception exp)
                {
                    memoryMapping.ReleaseReceiveSemaphore();
                    Console.WriteLine("收到虚拟机状态信息异常，异常为：" + exp.Message);
                }
            }
        }
        public void StartMessageReceiver()
        {
            var task = new Task(() =>
            {
                ReceiveMessage();
            });
            task.Start();
        }
    }
}
