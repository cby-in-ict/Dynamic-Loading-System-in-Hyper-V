using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace Perf_Detector
{
    public class MemSpoofer
    {
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
        public WinPerfCounter MEMPerfCounter = new WinPerfCounter();
        private System.Timers.Timer RUtimer;

        public MemSpoofer()
        {
            MEMPerfCounter.initAllCounterValue();
            Thread.Sleep(100);
            Thread th = new Thread(new ThreadStart(RefreshStatesByTime)); //创建线程                     
            th.Start(); //启动线程
        }
        public void RefreshStatesByTime()
        {
            // 创建一个100ms定时的定时器
            RUtimer = new System.Timers.Timer(100);    // 参数单位为ms
                                                       // 定时时间到，处理函数为OnTimedUEvent(...)
            RUtimer.Elapsed += RefreshMEMArg;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }
        public void RefreshMEMArg(object sender, ElapsedEventArgs e)
        {
            try
            {
                MEMAvailable = MEMPerfCounter.getMemAvailable();
                MEMCommited = MEMPerfCounter.getMemCommited();
                MEMCommitLimit = MEMPerfCounter.getMemCommitLimit();
                MEMCommitedPerc = MEMPerfCounter.getMemCommitedPerc();
                MEMPoolPaged = MEMPerfCounter.getMemPoolPaged();
                MEMPoolNonPaged = MEMPerfCounter.getMemPoolNonPaged();
                MEMCached = MEMPerfCounter.getMemCachedBytes();
                PageFile = MEMPerfCounter.getPageFile();
                PagesPerSec = MEMPerfCounter.getPagesPerSec();
                PageFaultsPerSec = MEMPerfCounter.getPageFaultsPerSec();
                PageWritesPerSec = MEMPerfCounter.getPageWritesPerSec();
                PageReadsPerSec = MEMPerfCounter.getPageReadsPerSec();
                PagesInputPerSec = MEMPerfCounter.getPageInputsPerSec();
                PagesOutputPerSec = MEMPerfCounter.getPageOutputsPerSec();
                return;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "刷新MEM参数异常！");
                return;
            }
        }
        public void RefreshMEMArg()
        {
            try
            {
                MEMAvailable = MEMPerfCounter.getMemAvailable();
                MEMCommited = MEMPerfCounter.getMemCommited();
                MEMCommitLimit = MEMPerfCounter.getMemCommitLimit();
                MEMCommitedPerc = MEMPerfCounter.getMemCommitedPerc();
                MEMPoolPaged = MEMPerfCounter.getMemPoolPaged();
                MEMPoolNonPaged = MEMPerfCounter.getMemPoolNonPaged();
                MEMCached = MEMPerfCounter.getMemCachedBytes();
                PageFile = MEMPerfCounter.getPageFile();
                PagesPerSec = MEMPerfCounter.getPagesPerSec();
                PageFaultsPerSec = MEMPerfCounter.getPageFaultsPerSec();
                PageWritesPerSec = MEMPerfCounter.getPageWritesPerSec();
                PageReadsPerSec = MEMPerfCounter.getPageReadsPerSec();
                PagesInputPerSec = MEMPerfCounter.getPageInputsPerSec();
                PagesOutputPerSec = MEMPerfCounter.getPageOutputsPerSec();
                return;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "刷新MEM参数异常！");
                return;
            }
        }

    }
}
