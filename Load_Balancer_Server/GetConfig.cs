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
    public class VMConfig
    {
        public string VMName { set; get; }
        public bool Installed { set; get; }
        public UInt64 MemorySize { set; get; }
        public int CPUNum { set; get; }
        public int CPULimt { set; get; }
        //public VMConfig(string _VMName, bool _Installed, UInt64 _MemorySize, int _CPUNum, int _CPULimit)
        //{
        //    VMName = _VMName;
        //    Installed = _Installed;
        //    MemorySize = _MemorySize;
        //    CPUNum = _CPUNum;
        //    CPULimt = _CPULimit;
        //}
    }

    public class SysConfig
    {
        public string BalanceStrategy { set; get; }
        public int CPUDefaultCount { set; get; }
        public UInt64 MemoryDefaultSize { set; get; }
        public int CPUDefaultLimit { set; get; }
        //public SysConfig(string _BalanceStrategy, int _CPUDefaultCount, UInt64 _MemoryDefaultSize, int _CPUDefaultLimit)
        //{
        //    BalanceStrategy = _BalanceStrategy;
        //    CPUDefaultCount = _CPUDefaultCount;
        //    MemoryDefaultSize = _MemoryDefaultSize;
        //    CPUDefaultLimit = _CPUDefaultLimit;
        //}
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

    public class GetConfig
    {
        public static string ConfigFileRelPath = @"..\..\..";
        const string VMConfigName = "VMConfig.json";
        const string SysConfigName = "SystemConfig.json";
        const string ProcessConfigName = "ProcessWhiteList.json";
        public static string LocalVMProcessInfoPath =  Path.Combine(ConfigFileRelPath, "LocalVMProcConfig.json");
        public static string NetVM1ProcessInfoPath = Path.Combine(ConfigFileRelPath, "NetVM1ProcConfig.json");
        public static string NetVM2ProcessInfoPath = Path.Combine(ConfigFileRelPath, "NetVM2ProcConfig.json");
        string VMconfigPath = Path.Combine(ConfigFileRelPath, VMConfigName);
        string SysConfigPath = Path.Combine(ConfigFileRelPath, SysConfigName);
        string ProcessConfigPath = Path.Combine(ConfigFileRelPath, ProcessConfigName);
        public static string VMStateJsonPath = @"..\UsefulFile\VMState.json";

        // VMConfig 
       
        
        public SysConfig currentSysConfig = new SysConfig();
        public MpcVMInfo currentMpcVMInfo = new MpcVMInfo();
        public VMConfig GetVMConfig(string VMName)
        {
            VMConfig currentVMConfig = new VMConfig();
            using ( System.IO.StreamReader file = System.IO.File.OpenText(VMconfigPath))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);

                    if (o.ContainsKey(VMName))
                    {
                        currentVMConfig.VMName = VMName;
                        JObject vmObj = ((JObject)o[VMName]);
                        currentVMConfig.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        currentVMConfig.CPUNum = Convert.ToInt32(vmObj["OriginalCPUCount"].ToString());
                        currentVMConfig.MemorySize = Convert.ToUInt64(vmObj["PowerOnMemorySize"].ToString());
                        currentVMConfig.CPULimt = Convert.ToInt32(vmObj["CPULimit"].ToString());
                    }
                    else
                    {
                        Console.WriteLine("配置文件中无该虚拟机：" + VMName);
                        return null;
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

        public MpcVMInfo GetMpcVMInfo(string VMStateJsonPath, string VMName)
        {
            using (System.IO.StreamReader file = System.IO.File.OpenText(VMStateJsonPath))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    if (VMName == "LocalVM" && o.ContainsKey("LocalVM"))
                    {
                        JObject vmObj = ((JObject)o["LocalVM"]);
                        currentMpcVMInfo.VMName = vmObj["VMName"].ToString();
                        currentMpcVMInfo.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        currentMpcVMInfo.VMPath = vmObj["VMPath"].ToString();
                        currentMpcVMInfo.DefaultUser = vmObj["DefaultUser"].ToString();
                        currentMpcVMInfo.Password = vmObj["Password"].ToString();
                        currentMpcVMInfo.IPAddress = vmObj["IPAddress"].ToString();
                        currentMpcVMInfo.StateSnapID = vmObj["StateSnapID"].ToString();
                        currentMpcVMInfo.Description = vmObj["Description"].ToString();
                        currentMpcVMInfo.RenamePC = Convert.ToBoolean(vmObj["RenamePC"].ToString());
                        return currentMpcVMInfo;
                    }
                    if (VMName == "NetVM1" && o.ContainsKey("NetVM1"))
                    {
                        JObject vmObj = ((JObject)o["NetVM1"]);
                        currentMpcVMInfo.VMName = vmObj["VMName"].ToString();
                        currentMpcVMInfo.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        currentMpcVMInfo.VMPath = vmObj["VMPath"].ToString();
                        currentMpcVMInfo.DefaultUser = vmObj["DefaultUser"].ToString();
                        currentMpcVMInfo.Password = vmObj["Password"].ToString();
                        currentMpcVMInfo.IPAddress = vmObj["IPAddress"].ToString();
                        currentMpcVMInfo.StateSnapID = vmObj["StateSnapID"].ToString();
                        currentMpcVMInfo.Description = vmObj["Description"].ToString();
                        currentMpcVMInfo.RenamePC = Convert.ToBoolean(vmObj["RenamePC"].ToString());
                        return currentMpcVMInfo;
                    }
                    if (VMName == "NetVM2" && o.ContainsKey("NetVM2"))
                    {
                        JObject vmObj = ((JObject)o["NetVM2"]);
                        currentMpcVMInfo.VMName = vmObj["VMName"].ToString();
                        currentMpcVMInfo.Installed = Convert.ToBoolean(vmObj["Installed"].ToString());
                        currentMpcVMInfo.VMPath = vmObj["VMPath"].ToString();
                        currentMpcVMInfo.DefaultUser = vmObj["DefaultUser"].ToString();
                        currentMpcVMInfo.Password = vmObj["Password"].ToString();
                        currentMpcVMInfo.IPAddress = vmObj["IPAddress"].ToString();
                        currentMpcVMInfo.StateSnapID = vmObj["StateSnapID"].ToString();
                        currentMpcVMInfo.Description = vmObj["Description"].ToString();
                        currentMpcVMInfo.RenamePC = Convert.ToBoolean(vmObj["RenamePC"].ToString());
                        return currentMpcVMInfo;
                    }
                    else
                        return null;
                }
            }
        }
    }
}
