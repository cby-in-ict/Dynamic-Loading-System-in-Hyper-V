using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using Microsoft;
using Microsoft.Samples.HyperV.Common;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections;

/* In this part, I partialy refer to MultiPC VirutalMachine.cs, copyright reserved by ict bis lab, based our code, and I revised it */

namespace Load_Balancer_Server
{
    #region Host可以直接获得的性能指标
    public struct PerformanceSetting
        {
        public String Name;
        public String ElementName;
        public String InstanceID;
        public UInt16 NumberOfProcessors;
        public UInt16 EnabledState;
        public UInt16 HealthState;
        public UInt16 ProcessorLoad;
        public UInt16[] ProcessorLoadHistory;
        public UInt64 MemoryUsage;
        public String GuestOperatingSystem;
        // public Int32 MemoryAvailable;
        //public Int32 AvailableMemoryBuffer;
        public UInt64 CPU_Reservation;//保留    0-100000
        public UInt64 CPU_Limit;//限制    0-100000
        public UInt32 CPU_Weight;//分配权重   1-10000
        public UInt64 RAM_VirtualQuantity;
        public UInt32 RAM_Weight;//分配权重   1-10000
        public bool RAM_DynamicMemoryEnabled;//动态内存
        public float CpuPercentage;
        }
    #endregion

    public class VirtualMachine
        {
            const int StartID = 2;
            const int ForceShutDownID = 3;
            const int ShutDownID = 4;
            const int RebootID = 10;

            ManagementScope scope;
            ManagementObject managementService;
            ManagementObject virtualMachine;

            public string vmName;
            public PerformanceSetting performanceSetting;

            public enum VirtualMachineStatus
            {
                PowerOff = 0,
                RequestPowerOn = 1,
                PowerOn = 2
            }
            public VirtualMachineStatus vmStatus { set; get; }

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
            #region 获得或者刷新虚拟机的Performance
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
                performanceSetting.ProcessorLoad = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("ProcessorLoad"));
                
                UInt16[] ProcessorHistoryInfo = (UInt16[])virtualSystemSetting.GetPropertyValue("ProcessorLoadHistory");
                performanceSetting.ProcessorLoadHistory = ProcessorHistoryInfo;

                performanceSetting.MemoryUsage = Convert.ToUInt64(virtualSystemSetting.GetPropertyValue("MemoryUsage"));
                performanceSetting.GuestOperatingSystem = virtualSystemSetting.GetPropertyValue("GuestOperatingSystem") + "";
                //performanceSetting.AvailableMemoryBuffer = Convert.ToInt32(virtualSystemSetting.GetPropertyValue("AvailableMemoryBuffer"));

                using (ManagementObject detailVMSetting = WmiUtilities.GetVirtualMachineSettings(virtualMachine))
                {
                    using (ManagementObjectCollection processorSettingDatas = detailVMSetting.GetRelated("Msvm_ProcessorSettingData"))
                    using (ManagementObject processorSettingData = WmiUtilities.GetFirstObjectFromCollection(processorSettingDatas))
                    {
                        performanceSetting.CPU_Reservation = Convert.ToUInt64(processorSettingData.GetPropertyValue("Reservation"));
                        performanceSetting.CPU_Limit = Convert.ToUInt64(processorSettingData.GetPropertyValue("Limit"));
                        performanceSetting.CPU_Weight = Convert.ToUInt32(processorSettingData.GetPropertyValue("Weight"));
                    }
                    using (ManagementObjectCollection memorySettingDatas = detailVMSetting.GetRelated("Msvm_MemorySettingData"))
                    using (ManagementObject memorySettingData = WmiUtilities.GetFirstObjectFromCollection(memorySettingDatas))
                    {
                        performanceSetting.RAM_VirtualQuantity = Convert.ToUInt64(memorySettingData.GetPropertyValue("VirtualQuantity"));
                        performanceSetting.RAM_Weight = Convert.ToUInt32(memorySettingData.GetPropertyValue("Weight"));
                        performanceSetting.RAM_DynamicMemoryEnabled = Convert.ToBoolean(memorySettingData.GetPropertyValue("DynamicMemoryEnabled"));
                    }
                }

                performanceSetting.CpuPercentage = (((float)performanceSetting.CPU_Limit/100000) * performanceSetting.NumberOfProcessors);
                performanceSetting.CpuPercentage = performanceSetting.CpuPercentage / 4;
                return performanceSetting;
            }
            #endregion

            public bool IsPowerOn()
            {
                ManagementObjectCollection virtualSystemSettings = virtualMachine.GetRelated("Msvm_SummaryInformation");
                ManagementObject virtualSystemSetting = WmiUtilities.GetFirstObjectFromCollection(virtualSystemSettings);
                UInt16 EnabledState = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("EnabledState"));
                return (EnabledState == 2);
            }

            public bool IsPowerOff()
            {
                ManagementObjectCollection virtualSystemSettings = virtualMachine.GetRelated("Msvm_SummaryInformation");
                ManagementObject virtualSystemSetting = WmiUtilities.GetFirstObjectFromCollection(virtualSystemSettings);
                UInt16 EnabledState = Convert.ToUInt16(virtualSystemSetting.GetPropertyValue("EnabledState"));
                if (EnabledState == 3)
                {
                    vmStatus = VirtualMachineStatus.PowerOff;
                    return true;
                }
                else
                    return false;
            }

            public bool PowerOn()
            {
                bool result = false;
                using (ManagementBaseObject inParams = virtualMachine.GetMethodParameters("RequestStateChange"))
                {
                    inParams["RequestedState"] = StartID;
                    using (ManagementBaseObject outParams = virtualMachine.InvokeMethod(
                                "RequestStateChange", inParams, null))
                    {
                        try
                        {
                            result = WmiUtilities.ValidateOutput(outParams, scope);
                        }
                        catch (ManagementException e)
                        {
                            Console.WriteLine("无法开机，异常为:" + e.Message);
                        }
                    }
                }
                return result;
            }

        public bool PowerOff()
        {
            bool result = false;
            using (ManagementBaseObject inParams = virtualMachine.GetMethodParameters("RequestStateChange"))
            {
                inParams["RequestedState"] = ShutDownID;
                using (ManagementBaseObject outParams = virtualMachine.InvokeMethod(
                            "RequestStateChange", inParams, null))
                {
                    try
                    {
                        result = WmiUtilities.ValidateOutput(outParams, scope);
                    }
                    catch (ManagementException e)
                    {
                        Console.WriteLine("");
                    }
                }
            }
            if (result)
                vmStatus = VirtualMachineStatus.PowerOff;
            return result;
        }

        public bool ModifySettingData(String settingName, String settingData)
            {
                bool result = false;

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

