using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace Perf_Detector
{
    public class CpuSpoofer
    {
        public static int CPUCount = 0;
        public static float ProcessorQueueLength = 0;
        public static float CPUProcessorTime { get; set; }
        public static float CPUPrivilegedTime { get; set; }
        public static float CPUInterruptTime { get; set; }
        public static float CPUDPCTime { get; set; }
        public static float ThreadCount { get; set; }
        public static WinPerfCounter WinPerfCounter = new WinPerfCounter();
        private static System.Timers.Timer RUtimer;

        CpuSpoofer()
        {
            Thread th = new Thread(new ThreadStart(RefreshStatesByTime)); //创建线程                     
            th.Start(); //启动线程
        }
        public static void RefreshStatesByTime()
        {
            // 创建一个100ms定时的定时器
            RUtimer = new System.Timers.Timer(1000);    // 参数单位为ms
                                                       // 定时时间到，处理函数为OnTimedUEvent(...)
            RUtimer.Elapsed += RefreshCPUArg;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }
        public static void RefreshCPUArg(object sender, ElapsedEventArgs e)
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
