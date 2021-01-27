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
        public static string currentUsedVM = "GetOff";
        public static VirtualMachine LocalVM;
        public static VMConfig LocalVMConfig;
        public static VirtualMachine NetVM1;
        public static VMConfig NeyVM1Config;
        public static VirtualMachine NetVM2;
        public static VMConfig NeyVM2Config;
        public VMState(string MpcVMConfigPath)
        {
            GetConfig configParser = new GetConfig();
            configParser.currentMpcVMInfo = configParser.GetMpcVMInfo(MpcVMConfigPath, "LocalVM");
            if (configParser.currentMpcVMInfo.Installed == true)
            {
                ManagementScope scope;
                ManagementObject managementService;

                scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                LocalVM = new VirtualMachine("LocalVM", scope, managementService);
            }
            configParser.currentMpcVMInfo = configParser.GetMpcVMInfo(MpcVMConfigPath, "NetVM1");
            if (configParser.currentMpcVMInfo.Installed == true)
            {
                ManagementScope scope;
                ManagementObject managementService;

                scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                NetVM1 = new VirtualMachine("NetVM1", scope, managementService);
            }
            configParser.currentMpcVMInfo = configParser.GetMpcVMInfo(MpcVMConfigPath, "NetVM2");
            if (configParser.currentMpcVMInfo.Installed == true)
            {
                ManagementScope scope;
                ManagementObject managementService;

                scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                NetVM2 = new VirtualMachine("NetVM2", scope, managementService);
            }

            LocalVMConfig = configParser.GetVMConfig("LocalVM");
            NeyVM1Config = configParser.GetVMConfig("NetVM1");
            NeyVM2Config = configParser.GetVMConfig("NetVM2");
        }

        public static void SetVMState(string VMName, string State)
        {
            if (VMName.Contains("LocalVM"))
            {
                Console.WriteLine("here");
                if (LocalVM == null && State != "Install")
                {
                    Console.WriteLine("LocalVM未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        LocalVM.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        LocalVM.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        // TODO:加速开机流程
                        break;
                    case "PowerOn":
                        LocalVM.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        Console.WriteLine("LocalVM转换为开机状态");
                        // TODO:结束开机加速流程，返回初始设置
                        break;
                    case "UnInstall":
                        LocalVM = null;
                        break;
                    case "Install":
                        ManagementScope scope;
                        ManagementObject managementService;

                        scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                        managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                        LocalVM = new VirtualMachine("LocalVM", scope, managementService);
                        break;
                    default:
                        break;
                }
            }
            else if (VMName.Contains("NetVM1"))
            {
                if (NetVM1 == null && State != "Install")
                {
                    Console.WriteLine("NetVM1未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        NetVM1.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        NetVM1.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        break;
                    case "PowerOn":
                        NetVM1.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        break;
                    case "UnInstall":
                        NetVM1 = null;
                        break;
                    case "Install":
                        ManagementScope scope;
                        ManagementObject managementService;

                        scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                        managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                        NetVM1 = new VirtualMachine("NetVM1", scope, managementService);
                        break;
                    default:
                        break;
                }
            }
            else if (VMName.Contains("NetVM2"))
            {
                if (NetVM2 == null && State != "Install")
                {
                    Console.WriteLine("NetVM2未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        NetVM2.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        NetVM2.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        break;
                    case "PowerOn":
                        NetVM2.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        break;
                    case "UnInstall":
                        NetVM2 = null;
                        break;
                    case "Install":
                        ManagementScope scope;
                        ManagementObject managementService;

                        scope = new ManagementScope(@"\\.\root\virtualization\v2", null);
                        managementService = WmiUtilities.GetVirtualMachineManagementService(scope);
                        NetVM2 = new VirtualMachine("NetVM2", scope, managementService);
                        break;
                    default:
                        break;
                }
            }
            else
                return;
            return;
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
                    }
                }
                catch (Exception exp)
                {
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
