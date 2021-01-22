//#define PerfAnalysisTest
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Perf_Transfer;

/* 
 * This class analyse performance in the VM, when detect shortage in Mem and CPU resource,
 * There will be a signal when detect problem, then call the host.
 */
namespace Perf_Detector_Client
{
    public class Perf_Analysis
    {
        // The class used to transfer to Server
        public VMPerf perf_Transfer = new VMPerf();

        public bool MEMAlarm = false;
        public bool CPUAlarm = false;
        public float CPU_K { get; set; }
        public float MEM_K { get; set; }
        public string VMName { get; set; }

        public MemSpoofer memSpoofer;
        public CpuSpoofer cpuSpoofer;
        public ProcessInfo ProcessInfo = new ProcessInfo();
        private System.Timers.Timer RUtimer;

        public Perf_Analysis()
        {
            memSpoofer = new MemSpoofer();
            cpuSpoofer = new CpuSpoofer();
            memSpoofer.RefreshMEMArg();
            cpuSpoofer.RefreshCPUArg();
            bool ret = SetPerfTransfer(memSpoofer, cpuSpoofer);
            bool vmIDRet = GetVMId(@"C:\VMID.txt");
            if (!vmIDRet)
                VMName = "Unknown";
        }

        public bool GetVMId(string vmIDFilePath)
        {
            try
            {
                if (!File.Exists(vmIDFilePath))
                    return false;
                VMName = File.ReadAllText(vmIDFilePath);
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        public bool SetPerfTransfer(MemSpoofer memSpf, CpuSpoofer cpuSpf)
        {
            try
            {
                Thread.Sleep(300);
                perf_Transfer.VMName = VMName;
                perf_Transfer.CPUCount = cpuSpf.CPUCount;
                perf_Transfer.ProcessorQueueLength = cpuSpf.ProcessorQueueLength;
                perf_Transfer.CPUProcessorTime = cpuSpf.CPUProcessorTime;
                perf_Transfer.CPUPrivilegedTime = cpuSpf.CPUPrivilegedTime;
                perf_Transfer.CPUInterruptTime = cpuSpf.CPUInterruptTime;
                perf_Transfer.CPUDPCTime = cpuSpf.CPUDPCTime;
                perf_Transfer.MEMAvailable = memSpf.MEMAvailable;
                perf_Transfer.MEMCommited = memSpf.MEMCommited;
                perf_Transfer.MEMCommitLimit = memSpf.MEMCommitLimit;
                perf_Transfer.MEMCommitedPerc = memSpf.MEMCommitedPerc;
                perf_Transfer.MEMCached = memSpf.MEMCached;
                perf_Transfer.MEMPoolPaged = memSpf.MEMPoolPaged;
                perf_Transfer.MEMPoolNonPaged = memSpf.MEMPoolNonPaged;
                perf_Transfer.PageFile = memSpf.PageFile;
                perf_Transfer.PagesPerSec = memSpf.PagesPerSec;
                perf_Transfer.PageFaultsPerSec = memSpf.PageFaultsPerSec;
                perf_Transfer.PageReadsPerSec = memSpf.PageReadsPerSec;
                perf_Transfer.PageWritesPerSec = memSpf.PageWritesPerSec;
                perf_Transfer.PagesInputPerSec = memSpf.PagesInputPerSec;
                perf_Transfer.PagesOutputPerSec = memSpf.PagesOutputPerSec;
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "SetPerfTransfer异常");
                return false;
            }

        }
        public enum MemoryCondition
        {
            OK = 0, FULL = 1, NEED = 2, PageFault = 4, CacheMiss = 7
        }

        public void RefreshStatesByTime()
        {
            // 创建一个100ms定时的定时器
            RUtimer = new System.Timers.Timer(100);    // 参数单位为ms
                                                       // 定时时间到，处理函数为OnTimedUEvent(...)
            RUtimer.Elapsed += isRaiseAlarm;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }

        public void isRaiseAlarm(object sender, ElapsedEventArgs e)
        {
            MemoryCondition MEMState = CheckMEM();
            CPUAlarm = CheckCPU();

        }

        public MemoryCondition CheckMEM()
        {
            if (memSpoofer.MEMAvailable < 1000)
            {
                return MemoryCondition.FULL;
            }
            return MemoryCondition.OK;
        }

        public bool CheckCPU()
        {
            if (cpuSpoofer.CPUProcessorTime > 90)
            {
                if (cpuSpoofer.CPUPrivilegedTime < 80)
                    return false;
                else
                    return true;
            }
            return true;
        }
    }
#if PerfAnalysisTest
    class test
    {
        static void Main(string[] args)
        {
            Perf_Analysis perf_Analysis = new Perf_Analysis();
        }
    }
#endif
}
