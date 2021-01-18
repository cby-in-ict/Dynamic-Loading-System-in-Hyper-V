using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace Perf_Detector_Client
{
    public class CpuSpoofer
    {
        public int CPUCount = 0;
        public float ProcessorQueueLength = 0;
        public float CPUProcessorTime { get; set; }
        public float CPUPrivilegedTime { get; set; }
        public float CPUInterruptTime { get; set; }
        public float CPUDPCTime { get; set; }
        public float ThreadCount { get; set; }
        public  WinPerfCounter WinPerfCounter = new WinPerfCounter();
        private  System.Timers.Timer RUtimer;

        public CpuSpoofer()
        {
            WinPerfCounter.initAllCounterValue();
            Thread.Sleep(100);
            Thread th = new Thread(new ThreadStart(RefreshStatesByTime)); //创建线程                     
            th.Start(); //启动线程
        }
        public void RefreshStatesByTime()
        {
            // 创建一个100ms定时的定时器
            RUtimer = new System.Timers.Timer(100);    // 参数单位为ms
                                                       // 定时时间到，处理函数为OnTimedUEvent(...)
            RUtimer.Elapsed += RefreshCPUArg;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }
        public void RefreshCPUArg(object sender, ElapsedEventArgs e)
        {
            try
            {
                CPUProcessorTime = WinPerfCounter.getProcessorCpuTime();
                ProcessorQueueLength = WinPerfCounter.getProcessorQueueLengh();
                CPUPrivilegedTime = WinPerfCounter.getCpuPrivilegedTime();
                CPUInterruptTime = WinPerfCounter.getThreadCount();
                ThreadCount = WinPerfCounter.getThreadCount();
                return;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "刷新CPU参数异常！");
                return;
            }
        }

        public void RefreshCPUArg()
        {
            try
            {
                CPUProcessorTime = WinPerfCounter.getProcessorCpuTime();
                ProcessorQueueLength = WinPerfCounter.getProcessorQueueLengh();
                CPUPrivilegedTime = WinPerfCounter.getCpuPrivilegedTime();
                CPUInterruptTime = WinPerfCounter.getThreadCount();
                ThreadCount = WinPerfCounter.getThreadCount();
                return;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "刷新CPU参数异常！");
                return;
            }
        }
    }
}
