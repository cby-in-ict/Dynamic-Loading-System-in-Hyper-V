using System;
using System.Collections.Generic;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace HyperVWcfTransport
{
    class GetConfig
    {
        public static string ConfigFileRelPath = "..\\";
        const string VMConfigName = "";
        const string SysConfigName = "";
        const string ProcessConfigName = "";
        string VMconfigPath = Path.Combine(ConfigFileRelPath, VMConfigName);
        string SysConfigPath = Path.Combine(ConfigFileRelPath, SysConfigName);
        string ProcessConfigPath = Path.Combine(ConfigFileRelPath, ProcessConfigName);

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
            public string VMName { set; get; }
            public bool Installed { set; get; }
            public UInt64 MemorySize { set; get; }
            public int CPUNum { set; get; }
            public int CPULimt { set; get; }
            public SysConfig(string _VMName, bool _Installed, UInt64 _MemorySize, int _CPUNum, int _CPULimit)
            {
                VMName = _VMName;
                Installed = _Installed;
                MemorySize = _MemorySize;
                CPUNum = _CPUNum;
                CPULimt = _CPULimit;
            }
        }

        public VMConfig currentVMConfig;
        public SysConfig currentSysConfig;
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

        public 
    }
}
