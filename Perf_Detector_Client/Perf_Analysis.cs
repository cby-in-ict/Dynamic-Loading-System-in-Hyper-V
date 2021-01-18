#define PerfAnalysisTest
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

/* 
 * This class analyse performance in the VM, when detect shortage in Mem and CPU resource,
 * There will be a signal when detect problem, then call the host.
 */
namespace Perf_Detector
{
    [Serializable]
    public class Perf_Transfer
    {
        public int CPUCount = 0;
        public float ProcessorQueueLength { get; set; }
        public float CPUProcessorTime { get; set; }
        public float CPUPrivilegedTime { get; set; }
        public float CPUInterruptTime { get; set; }
        public float CPUDPCTime { get; set; }
        public float MEMAvailable { get; set; }
        public float MEMCommited { get; set; }
        public float MEMCommitLimit { get; set; }
        public float MEMCommitedPerc { get; set; }
        public float MEMPoolPaged { get; set; }
        public float MEMPoolNonPaged { get; set; }
        public float MEMCached { get; set; }
        public float PageFile { get; set; }
        public float PagesPerSec { get; set; }
        public float PageFaultsPerSec { get; set; }
        public float PageReadsPerSec { get; set; }
        public float PageWritesPerSec { get; set; }
        public float PagesInputPerSec { get; set; }
        public float PagesOutputPerSec { get; set; }
    }
    public class Perf_Analysis
    {
        // The class used to transfer to Server
        public Perf_Transfer perf_Transfer = new Perf_Transfer();

        public bool MEMAlarm = false;
        public bool CPUAlarm = false;
        public float CPU_K { get; set; }
        public float MEM_K { get; set; }

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
        }

        public bool SetPerfTransfer(MemSpoofer memSpf, CpuSpoofer cpuSpf)
        {
            try
            {
                Thread.Sleep(300);
                perf_Transfer.CPUCount = cpuSpf.CPUCount;
                perf_Transfer.ProcessorQueueLength = cpuSpf.ProcessorQueueLength;
                this.perf_Transfer.CPUProcessorTime = cpuSpf.CPUProcessorTime;
                this.perf_Transfer.CPUPrivilegedTime = cpuSpf.CPUPrivilegedTime;
                this.perf_Transfer.CPUInterruptTime = cpuSpf.CPUInterruptTime;
                this.perf_Transfer.CPUDPCTime = cpuSpf.CPUDPCTime;
                perf_Transfer.MEMAvailable = memSpf.MEMAvailable;
                perf_Transfer.MEMCommited = memSpf.MEMCommited;
                perf_Transfer.MEMCommitLimit = memSpf.MEMCommitLimit;
                this.perf_Transfer.MEMCommitedPerc = memSpf.MEMCommitedPerc;
                this.perf_Transfer.MEMCached = memSpf.MEMCached;
                this.perf_Transfer.MEMPoolPaged = memSpf.MEMPoolPaged;
                this.perf_Transfer.MEMPoolNonPaged = memSpf.MEMPoolNonPaged;
                this.perf_Transfer.PageFile = memSpf.PageFile;
                this.perf_Transfer.PagesPerSec = memSpf.PagesPerSec;
                this.perf_Transfer.PageFaultsPerSec = memSpf.PageFaultsPerSec;
                this.perf_Transfer.PageReadsPerSec = memSpf.PageReadsPerSec;
                this.perf_Transfer.PageWritesPerSec = memSpf.PageWritesPerSec;
                this.perf_Transfer.PagesInputPerSec = memSpf.PagesInputPerSec;
                this.perf_Transfer.PagesOutputPerSec = memSpf.PagesOutputPerSec;
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
            if (cpuSpoofer.ThreadCount > (cpuSpoofer.CPUCount * 2))
            {

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
