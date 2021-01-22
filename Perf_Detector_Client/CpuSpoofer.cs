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
        public  WinPerfCounter winPerfCounter = new WinPerfCounter();
        private  System.Timers.Timer RUtimer;

        public CpuSpoofer()
        {
            winPerfCounter.initAllCounterValue();

            Thread.Sleep(100);
            Thread th = new Thread(new ThreadStart(RefreshCpuStatesByTime)); //创建线程                     
            th.Start(); //启动线程
        }
        public void RefreshCpuStatesByTime()
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
                CPUProcessorTime = winPerfCounter.getProcessorCpuTime();
                ProcessorQueueLength = winPerfCounter.getProcessorQueueLengh();
                CPUPrivilegedTime = winPerfCounter.getCpuPrivilegedTime();
                CPUInterruptTime = winPerfCounter.getThreadCount();
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
                CPUProcessorTime = winPerfCounter.getProcessorCpuTime();
                ProcessorQueueLength = winPerfCounter.getProcessorQueueLengh();
                CPUPrivilegedTime = winPerfCounter.getCpuPrivilegedTime();
                CPUInterruptTime = winPerfCounter.getThreadCount();
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
