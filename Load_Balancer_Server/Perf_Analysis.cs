using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

/* 
 * This class analyse performance in the VM, when detect shortage in Mem and CPU resource,
 * There will be a signal when detect problem, then call the host.
 */
namespace Load_Balancer_Server
{
    //[Serializable]
    //public class Perf_Transfer
    //{
    //    public int CPUCount = 0;
    //    public float ProcessorQueueLength { get; set; }
    //    public float CPUProcessorTime { get; set; }
    //    public float CPUPrivilegedTime { get; set; }
    //    public float CPUInterruptTime { get; set; }
    //    public float CPUDPCTime { get; set; }
    //    public float MEMAvailable { get; set; }
    //    public float MEMCommited { get; set; }
    //    public float MEMCommitLimit { get; set; }
    //    public float MEMCommitedPerc { get; set; }
    //    public float MEMPoolPaged { get; set; }
    //    public float MEMPoolNonPaged { get; set; }
    //    public float MEMCached { get; set; }
    //    public float PageFile { get; set; }
    //    public float PagesPerSec { get; set; }
    //    public float PageFaultsPerSec { get; set; }
    //    public float PageReadsPerSec { get; set; }
    //    public float PageWritesPerSec { get; set; }
    //    public float PagesInputPerSec { get; set; }
    //    public float PagesOutputPerSec { get; set; }
    //}
    public class Perf_Analysis
    {
        public bool MEMAlarm = false;
        public bool CPUAlarm = false;
        public float CPU_K { get; set; }
        public float MEM_K { get; set; }
        //public float CPUUsage = 0;
        //public UInt64 MemoryUsage = 0;
        //public int ProcessorQueueLength = 0;
        //public float CacheMissRate = 0;
        //public UInt64 PageFaultCount = 0;
        /*
        public MemSpoofer MemSpoofer = new MemSpoofer();
        public CpuSpoofer CpuSpoofer = new CpuSpoofer();
        public ProcessInfo ProcessInfo = new ProcessInfo();
        private System.Timers.Timer RUtimer;

        public enum MemoryCondition 
        {
            OK=0, FULL=1, NEED=2, PageFault=4, CacheMiss=7
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
            if (MemSpoofer.MEMAvailable < 1000)
            {
                return MemoryCondition.FULL;
            }
            return MemoryCondition.OK;
        }

        public bool CheckCPU()
        {
            if (CpuSpoofer.CPUProcessorTime > 90)
            {
                if (CpuSpoofer.CPUPrivilegedTime < 80)
                    return false;
                else
                    return true;
            }
            if (CpuSpoofer.ThreadCount > (CpuSpoofer.CPUCount * 2))
            {
                
            }
            return true;
        }
        */
    }
}
