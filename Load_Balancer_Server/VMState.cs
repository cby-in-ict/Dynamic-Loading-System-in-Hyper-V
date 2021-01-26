using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using System.Threading;
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
        public static VirtualMachine LocalVM;
        public static VirtualMachine NetVM1;
        public static VirtualMachine NetVM2;
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
        }

        public static void SetVMState(string VMName, string State)
        {
            if (VMName == "LoaclVM")
            {
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
                        break;
                    case "PowerOn":
                        LocalVM.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
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
            else if (VMName == "NetVM1" && State != "Install")
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
            else if (VMName == "NetVM2" && State != "Install")
            {
                if (NetVM2 == null)
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
    }
}
