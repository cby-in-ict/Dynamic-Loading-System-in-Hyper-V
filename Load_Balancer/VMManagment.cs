using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using Microsoft;
using Microsoft.Samples.HyperV.Common;
using System.Windows;
using Microsoft.Samples.HyperV.IntegrationServices;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using System.Collections.ObjectModel;

/* In this part, I partialy refer to MultiPC BackgroundCode/VirutalMachine.cs, copyright reserved by ict bis lab, based our code, and I revised it */
namespace Load_Balancer
{
    public struct PerformanceSetting
    {
        public String Name;
        public String ElementName;
        public String InstanceID;
        public UInt16 NumberOfProcessors;
        public UInt16 EnabledState;
        public UInt16 HealthState;
        public String HostComputerSystemName;
        public bool SwapFilesInUse;
        public UInt16 ProcessorLoad;
        public UInt16[] ProcessorLoadHistory;
        public UInt64 MemoryUsage;
        public String GuestOperatingSystem;
        public Int32 MemoryAvailable;
        public Int32 AvailableMemoryBuffer;
        public UInt64 CPU_Reservation;//保留    0-100
        public UInt64 CPU_Limit;//限制    0-100
        public UInt32 CPU_Weight;//分配权重   1-10000
        public UInt32 RAM_Weight;//分配权重   1-10000
        public bool RAM_DynamicMemoryEnabled;//动态内存
    }

        public class VirtualMachine
        {
            const int StartID = 2;
            const int ForceShutDownID = 3;
            const int ShutDownID = 4;
            const int RebootID = 10;

            ManagementScope scope;
            ManagementObject managementService;
            ManagementObject virtualMachine;
            // ManagementObject snapshotService;

            public String vmName;
            public PerformanceSetting performanceSetting;

            public enum VirtualMachineStatus
            {
                PowerOff = 0,
                PowerOn = 1
            }
            private VirtualMachineStatus __vmStatus;
            public VirtualMachineStatus vmStatus
            {
                get => __vmStatus;
                set
                {}
            }

            public VirtualMachine(String _vmName, ManagementScope scopeM, ManagementObject managementServiceM)
            {
                vmName = _vmName;
                scope = scopeM;
                managementService = managementServiceM;
                try
                {
                    virtualMachine = WmiUtilities.GetVirtualMachine(vmName, scope);
                }
                catch (ManagementException)
                {
                    throw new ArgumentException(string.Format("The virtual machine \"{0}\" could not be found.", vmName));
                }
                if (virtualMachine != null)
                {
                    GetPerformanceSetting();
                }
                vmStatus = (performanceSetting.EnabledState == 2) ? VirtualMachineStatus.PowerOn : VirtualMachineStatus.PowerOff;
            }
            ~VirtualMachine()
            {

            }

            public PerformanceSetting GetPerformanceSetting()
            {
                ManagementObjectCollection virtualSystemSettings = virtualMachine.GetRelated("Msvm_SummaryInformation");
                ManagementObject virtualSystemSetting = WmiUtilities.GetFirstObjectFromCollection(virtualSystemSettings);

                //Msvm_SummaryInformation
                performanceSetting.Name = virtualSystemSetting.GetPropertyValue("Name") + "";
                performanceSetting.ElementName = virtualSystemSetting.GetPropertyValue("ElementName") + "";
                performanceSetting.InstanceID = virtualSystemSetting.GetPropertyValue("InstanceID") + "";
                performanceSetting.NumberOfProcessors = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("NumberOfProcessors"));
                performanceSetting.EnabledState = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("EnabledState"));
                performanceSetting.HealthState = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("HealthState"));
                performanceSetting.HostComputerSystemName = virtualSystemSetting.GetPropertyValue("HostComputerSystemName") + "";
                performanceSetting.SwapFilesInUse = Convert.ToBoolean(virtualSystemSetting.GetPropertyValue("SwapFilesInUse"));
                performanceSetting.ProcessorLoad = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("ProcessorLoad"));
                performanceSetting.MemoryUsage = Convert.ToUInt64(virtualSystemSetting.GetPropertyValue("MemoryUsage"));
                performanceSetting.GuestOperatingSystem = virtualSystemSetting.GetPropertyValue("GuestOperatingSystem") + "";
                performanceSetting.MemoryAvailable = Convert.ToInt32(virtualSystemSetting.GetPropertyValue("MemoryAvailable"));
                performanceSetting.AvailableMemoryBuffer = Convert.ToInt32(virtualSystemSetting.GetPropertyValue("AvailableMemoryBuffer"));
                using (ManagementObjectCollection processorSettingDatas = virtualSystemSetting.GetRelated("Msvm_ProcessorSettingData"))
                using (ManagementObject processorSettingData = WmiUtilities.GetFirstObjectFromCollection(processorSettingDatas))
                {
                    performanceSetting.CPU_Reservation = Convert.ToUInt64(processorSettingData.GetPropertyValue("Reservation"));
                    performanceSetting.CPU_Limit = Convert.ToUInt64(processorSettingData.GetPropertyValue("Limit"));
                    performanceSetting.CPU_Weight = Convert.ToUInt32(processorSettingData.GetPropertyValue("Weight"));
                }
                using (ManagementObjectCollection memorySettingDatas = virtualSystemSetting.GetRelated("Msvm_MemorySettingData"))
                using (ManagementObject memorySettingData = WmiUtilities.GetFirstObjectFromCollection(memorySettingDatas))
                {
                    performanceSetting.RAM_Weight = Convert.ToUInt32(memorySettingData.GetPropertyValue("Weight"));
                    performanceSetting.RAM_DynamicMemoryEnabled = Convert.ToBoolean(memorySettingData.GetPropertyValue("DynamicMemoryEnabled"));
                }

                return performanceSetting;
            }

            public bool IsPowerOn()
            {
                GetPerformanceSetting();
                return (performanceSetting.EnabledState == 2);
            }

            public bool IsPowerOff()
            {
                GetPerformanceSetting();
                if (performanceSetting.EnabledState == 3)
                {
                    vmStatus = VirtualMachineStatus.PowerOff;
                    return true;
                }
                else
                    return false;
            }

            public bool ModifySettingData(String settingName, String settingData)
            {
                bool result = false;
                if (!IsPowerOff())
                {
                    Console.WriteLine("关机状态下修改，非动态修改！");
                    return false;
                }

                using (ManagementObject virtualSystemSetting = WmiUtilities.GetVirtualMachineSettings(virtualMachine))
                {
                    if (settingName.Substring(0, 3) == "CPU")
                    {
                        settingName = settingName.Substring(4);
                        using (ManagementObjectCollection processorSettingDatas = virtualSystemSetting.GetRelated("Msvm_ProcessorSettingData"))
                        using (ManagementObject processorSettingData = WmiUtilities.GetFirstObjectFromCollection(processorSettingDatas))
                        {
                            if (processorSettingData[settingName].GetType() == typeof(UInt64))
                                processorSettingData[settingName] = Convert.ToUInt64(settingData);
                            else if (processorSettingData[settingName].GetType() == typeof(UInt32))
                                processorSettingData[settingName] = Convert.ToUInt32(settingData);

                            using (ManagementBaseObject inParams = managementService.GetMethodParameters("ModifyResourceSettings"))
                            {
                                inParams["ResourceSettings"] = new string[] { processorSettingData.GetText(TextFormat.WmiDtd20) };
                                using (ManagementBaseObject outParams = managementService.InvokeMethod(
                                    "ModifyResourceSettings", inParams, null))
                                {
                                    try
                                    {
                                        result = WmiUtilities.ValidateOutput(outParams, scope);
                                    }
                                    catch (ManagementException e)
                                    {
                                        Console.WriteLine(e.Message, "修改虚拟机CPU配置异常！");
                                    }
                                    return result;
                                }
                            }
                        }
                    }
                    else if (settingName.Substring(0, 3) == "RAM")
                    {
                        settingName = settingName.Substring(4);
                        using (ManagementObjectCollection memorySettingDatas = virtualSystemSetting.GetRelated("Msvm_MemorySettingData"))
                        using (ManagementObject memorySettingData = WmiUtilities.GetFirstObjectFromCollection(memorySettingDatas))
                        {
                            if (memorySettingData[settingName].GetType() == typeof(UInt64))
                                memorySettingData[settingName] = Convert.ToUInt64(settingData);
                            else if (memorySettingData[settingName].GetType() == typeof(UInt32))
                                memorySettingData[settingName] = Convert.ToUInt32(settingData);
                            else if (memorySettingData[settingName].GetType() == typeof(bool))
                                memorySettingData[settingName] = Convert.ToBoolean(settingData);

                            using (ManagementBaseObject inParams = managementService.GetMethodParameters("ModifyResourceSettings"))
                            {
                                inParams["ResourceSettings"] = new string[] { memorySettingData.GetText(TextFormat.WmiDtd20) };
                                using (ManagementBaseObject outParams = managementService.InvokeMethod(
                                    "ModifyResourceSettings", inParams, null))
                                {
                                    try
                                    {
                                        result = WmiUtilities.ValidateOutput(outParams, scope);
                                    }
                                    catch (ManagementException e)
                                    {
                                        Console.WriteLine(e.Message, "修改虚拟机CPU配置异常！");
                                    }
                                    return result;
                                }
                            }
                        }
                    }
                    return result;
                }
            }
        }
    }

