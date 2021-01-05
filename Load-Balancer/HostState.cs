using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;

namespace HyperVWcfTransport
{
    class HostState
    {
        Int64 AvailableMemory = 0;
        int CpuUsedPercentage = 0;
        public Int64 getAvailableMemory()
        {
            //用于查询一些如系统信息的管理对象
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(); 
            searcher.Query = new SelectQuery("Win32_PhysicalMemory", "", new string[] { "Capacity" });
            ManagementObjectCollection collection = searcher.Get();
            ManagementObjectCollection.ManagementObjectEnumerator em = collection.GetEnumerator();

            int capacity = 0;
            while (em.MoveNext())
            {
                ManagementBaseObject baseObj = em.Current;
                if (baseObj.Properties["Capacity"].Value != null)
                {
                    try
                    {
                        capacity += int.Parse(baseObj.Properties["Capacity"].Value.ToString());
                    }
                    catch
                    {
                        Console.WriteLine("内存查询异常！", "错误信息");
                        return -1;
                    }
                }
            }
            this.AvailableMemory = capacity;
            return capacity;
        }

        public int getCpuUsedPercentage()
        {
            return 1;
        }

        void getHostComputerInfo()
        {
            
        }
    }
}



namespace SystemInfo
{
    // System Info Class
    public class SystemInfo
    {
        private int m_ProcessorCount = 0;   //CPU个数 
        private PerformanceCounter pcCpuLoad;   //CPU计数器 
        private long m_PhysicalMemory = 0;   //物理内存 

        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;

        #region AIP声明 
        [DllImport("IpHlpApi.dll")]
        extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        #endregion

        #region 构造函数 
        // Constructor, init the perf counter 
        public SystemInfo()
        {
            // Init CPU perf counter
            pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            pcCpuLoad.MachineName = ".";
            pcCpuLoad.NextValue();

            // CPU number
            m_ProcessorCount = Environment.ProcessorCount;

            // Get  
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    m_PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
        }
        #endregion

        #region CPU个数 
        ///  
        /// 获取CPU个数 
        ///  
        public int ProcessorCount
        {
            get
            {
                return m_ProcessorCount;
            }
        }
        #endregion

        #region CPU占用率 
        ///  
        /// 获取CPU占用率 
        ///  
        public float CpuLoad
        {
            get
            {
                return pcCpuLoad.NextValue();
            }
        }
        #endregion

        #region 可用内存 
        ///  
        /// 获取可用内存 
        ///  
        public long MemoryAvailable
        {
            get
            {
                long availablebytes = 0;
                //ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfOS_Memory"); 
                //foreach (ManagementObject mo in mos.Get()) 
                //{ 
                //    availablebytes = long.Parse(mo["Availablebytes"].ToString()); 
                //} 
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
        }
        #endregion

        #region 物理内存 
        ///  
        /// 获取物理内存 
        ///  
        public long PhysicalMemory
        {
            get
            {
                return m_PhysicalMemory;
            }
        }
        #endregion

        #region 获得进程列表 
        ///  
        /// 获得进程列表 
        ///  
        public List<Process> GetProcessInfo()
        {
            List<Process> ret = new List<Process>();
            Process[] MyProcesses = Process.GetProcesses();
            foreach (Process MyProcess in MyProcesses)
            {
                ret.Add(MyProcess);
            }
            return ret;
        }
        ///  
        /// 获得特定进程信息 
        ///  
        /// 进程名称 
        //public Dictionary<string, string> GetProcessInfo(List<Process> ProcessList, string ProcesName)
        //{
            //List pInfo = new List();
            //Process[] processes = Process.GetProcessesByName(ProcessName);
            //foreach (Process instance in processes)
            //{
            //    try
            //    {
            //        pInfo.Add(new ProcessInfo(instance.Id,
            //            instance.ProcessName,
            //            instance.TotalProcessorTime.TotalMilliseconds,
            //            instance.WorkingSet64,
            //            instance.MainModule.FileName));
            //    }
            //    catch { }
        //    }
        //    return pInfo;
        //}
        #endregion

        #region 结束指定进程 
        /// 结束指定进程 
        /// 进程的 Process ID 
        public static void endProcess(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch { }
        }
        #endregion
    }
}
