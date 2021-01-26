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
        public static VirtualMachine LoaclVM;
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
                LoaclVM = new VirtualMachine("LocalVM", scope, managementService);
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
                if (LoaclVM == null)
                {
                    Console.WriteLine("LocalVM未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        VMState.LoaclVM.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        VMState.LoaclVM.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        break;
                    case "PowerOn":
                        VMState.LoaclVM.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        break;
                    default:
                        break;
                }
            }
            else if (VMName == "NetVM1")
            {
                if (NetVM1 == null)
                {
                    Console.WriteLine("NetVM1未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        VMState.NetVM1.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        VMState.NetVM1.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        break;
                    case "PowerOn":
                        VMState.NetVM1.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
                        break;
                    default:
                        break;
                }
            }
            else if (VMName == "NetVM2")
            {
                if (NetVM2 == null)
                {
                    Console.WriteLine("NetVM2未初始化");
                    return;
                }
                switch (State)
                {
                    case "PowerOff":
                        VMState.NetVM2.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOff;
                        break;
                    case "RequestPowerOn":
                        VMState.NetVM2.vmStatus = VirtualMachine.VirtualMachineStatus.RequestPowerOn;
                        break;
                    case "PowerOn":
                        VMState.NetVM2.vmStatus = VirtualMachine.VirtualMachineStatus.PowerOn;
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
