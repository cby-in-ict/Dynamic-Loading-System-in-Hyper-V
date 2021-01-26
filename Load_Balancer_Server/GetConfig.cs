using System;
using System.Collections.Generic;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace Load_Balancer_Server
{
    public class GetConfig
    {
        public static string ConfigFileRelPath = "..\\..\\..";
        const string VMConfigName = "VMConfig.json";
        const string SysConfigName = "SystemConfig.json";
        const string ProcessConfigName = "ProcessWhiteList.json";
        string VMconfigPath = Path.Combine(ConfigFileRelPath, VMConfigName);
        string SysConfigPath = Path.Combine(ConfigFileRelPath, SysConfigName);
        string ProcessConfigPath = Path.Combine(ConfigFileRelPath, ProcessConfigName);
        public static string VMStateJsonPath = @"..\UsefulFile\VMState.json";

        // VMConfig 
        public class VMConfig
        {
            public string VMName { set; get; }
            public bool Installed { set; get; }
            public UInt64 MemorySize { set; get; }
            public int CPUNum { set; get; }
            public int CPULimt { set; get; }
            public VMConfig(string _VMName, bool _Installed, UInt64 _MemorySize, int _CPUNum, int _CPULimit)
            {
                VMName = _VMName;
                Installed = _Installed;
                MemorySize = _MemorySize;
                CPUNum = _CPUNum;
                CPULimt = _CPULimit;
            }
        }

        public class SysConfig
        {
            public string BalanceStrategy { set; get; }
            public int CPUDefaultCount { set; get; }
            public UInt64 MemoryDefaultSize { set; get; }
            public int CPUDefaultLimit { set; get; }
            public SysConfig(string _BalanceStrategy, int _CPUDefaultCount, UInt64 _MemoryDefaultSize, int _CPUDefaultLimit)
            {
                BalanceStrategy = _BalanceStrategy;
                CPUDefaultCount = _CPUDefaultCount;
                MemoryDefaultSize = _MemoryDefaultSize;
                CPUDefaultLimit = _CPUDefaultLimit;
            }
        }

        public class MpcVMInfo
        {
            public string VMName { set; get; }
            public bool Installed { set; get; }
            public string VMPath { set; get; }
            public string Description { set; get; }
            public string DefaultUser { set; get; }
            public string Password { set; get; }
            public string IPAddress { set; get; }
            public string StateSnapID { set; get; }
            public bool RenamePC { set; get; }
        }

        public VMConfig currentVMConfig;
        public SysConfig currentSysConfig;
        public MpcVMInfo currentMpcVMInfo;
        public VMConfig GetVMConfig(string VMName)
        {
            using (System.IO.StreamReader file = System.IO.File.OpenText(VMconfigPath))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);

                    if (o.ContainsKey(VMName))
                    {
                        currentVMConfig.VMName = VMName;
                        JObject vmObj = ((JObject)o[VMName]);
                        currentVMConfig.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        currentVMConfig.CPUNum = Convert.ToInt32(vmObj["CPUCount"].ToString());
                        currentVMConfig.MemorySize = Convert.ToUInt64(vmObj["MemorySize"].ToString());
                        currentVMConfig.CPULimt = Convert.ToInt32(vmObj["CPULimit"].ToString());
                    }
                    else
                    {
                        Console.WriteLine("配置文件中无该虚拟机：" + VMName);
                    }
                }
            }
            return currentVMConfig;
        }

        public SysConfig GetSysConfig()
        {
            using (System.IO.StreamReader file = System.IO.File.OpenText(SysConfigPath))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    if (o.ContainsKey("BalanceStrategy"))
                        currentSysConfig.BalanceStrategy = o["BalanceStrategy"].ToString();
                    if (o.ContainsKey("CPUDefaultCount"))
                        currentSysConfig.CPUDefaultCount = Convert.ToInt32(o["CPUDefaultCount"].ToString());
                    if (o.ContainsKey("MemoryDefaultSize"))
                        currentSysConfig.MemoryDefaultSize = Convert.ToUInt64(o["MemoryDefaultSize"].ToString());
                    if (o.ContainsKey("CPUDefaultLimit"))
                        currentSysConfig.CPUDefaultLimit = Convert.ToInt32(o["CPUDefaultLimit"].ToString());
                    return currentSysConfig;
                }
            }
        }

        public MpcVMInfo GetMpcVMInfo(string VMStateJsonPath, string VMKind)
        {
            using (System.IO.StreamReader file = System.IO.File.OpenText(VMStateJsonPath))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    MpcVMInfo vmInfo = new MpcVMInfo();
                    if (VMKind == "LocalVM" && o.ContainsKey("LocalVM"))
                    {
                        JObject vmObj = ((JObject)o["LocalVM"]);
                        vmInfo.VMName = vmObj["VMName"].ToString();
                        vmInfo.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        vmInfo.VMPath = vmObj["VMPath"].ToString();
                        vmInfo.DefaultUser = vmObj["DefaultUser"].ToString();
                        vmInfo.Password = vmObj["Password"].ToString();
                        vmInfo.IPAddress = vmObj["IPAddress"].ToString();
                        vmInfo.StateSnapID = vmObj["StateSnapID"].ToString();
                        vmInfo.Description = vmObj["Description"].ToString();
                        vmInfo.RenamePC = Convert.ToBoolean(vmObj["RenamePC"].ToString());
                        return vmInfo;
                    }
                    if (VMKind == "NetVM1" && o.ContainsKey("NetVM1"))
                    {
                        JObject vmObj = ((JObject)o["NetVM1"]);
                        vmInfo.VMName = vmObj["VMName"].ToString();
                        vmInfo.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        vmInfo.VMPath = vmObj["VMPath"].ToString();
                        vmInfo.DefaultUser = vmObj["DefaultUser"].ToString();
                        vmInfo.Password = vmObj["Password"].ToString();
                        vmInfo.IPAddress = vmObj["IPAddress"].ToString();
                        vmInfo.StateSnapID = vmObj["StateSnapID"].ToString();
                        vmInfo.Description = vmObj["Description"].ToString();
                        vmInfo.RenamePC = Convert.ToBoolean(vmObj["RenamePC"].ToString());
                        return vmInfo;
                    }
                    if (VMKind == "NetVM2" && o.ContainsKey("NetVM2"))
                    {
                        JObject vmObj = ((JObject)o["NetVM2"]);
                        vmInfo.VMName = vmObj["VMName"].ToString();
                        vmInfo.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        vmInfo.VMPath = vmObj["VMPath"].ToString();
                        vmInfo.DefaultUser = vmObj["DefaultUser"].ToString();
                        vmInfo.Password = vmObj["Password"].ToString();
                        vmInfo.IPAddress = vmObj["IPAddress"].ToString();
                        vmInfo.StateSnapID = vmObj["StateSnapID"].ToString();
                        vmInfo.Description = vmObj["Description"].ToString();
                        vmInfo.RenamePC = Convert.ToBoolean(vmObj["RenamePC"].ToString());
                        return vmInfo;
                    }
                    else
                        return null;
                }
            }
        }
    }
}
