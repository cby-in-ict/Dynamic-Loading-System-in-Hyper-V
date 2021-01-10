using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;

/* This class is used to get performance counter, copyright preserved by cby */
namespace Perf_Detector
{
    public class WinPerfCounter
    {
        public string NodeName { get; set; }
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
        public float ProcessorQueueLengh { get; set; }
        public float HANDLECountCounter { get; set; }
        public float THREADCount { get; set; }
        public int CONTENTSwitches { get; set; }
        public int SYSTEMCalls { get; set; }
        public float NetTrafficSend { get; set; }
        public float NetTrafficReceive { get; set; }

        // 采样时间
        public DateTime SamplingTime { get; set; }
        // CPU占用率
        private PerformanceCounter cpuProcessorTime = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        // CPU特权程序时间
        private PerformanceCounter cpuPrivilegedTime = new PerformanceCounter("Processor", "% Privileged Time", "_Total");
        // CPU中断时间
        private PerformanceCounter cpuInterruptTime = new PerformanceCounter("Processor", "% Interrupt Time", "_Total");
        // DPC time
        private PerformanceCounter cpuDPCTime = new PerformanceCounter("Processor", "% DPC Time", "_Total");
        // 可用内存
        private PerformanceCounter memAvailable = new PerformanceCounter("Memory", "Available MBytes", null);
        // 内存的Committed Bytes
        private PerformanceCounter memCommited = new PerformanceCounter("Memory", "Committed Bytes", null);
        private PerformanceCounter memCommitLimit = new PerformanceCounter("Memory", "Commit Limit", null);
        private PerformanceCounter memCommitedPerc = new PerformanceCounter("Memory", "% Committed Bytes In Use", null);
        // 
        private PerformanceCounter memPollPaged = new PerformanceCounter("Memory", "Pool Paged Bytes", null);
        private PerformanceCounter memPollNonPaged = new PerformanceCounter("Memory", "Pool Nonpaged Bytes", null);
        // 内存对换区相关参数
        private PerformanceCounter memCached = new PerformanceCounter("Memory", "Cache Bytes", null);
        private PerformanceCounter pageFile = new PerformanceCounter("Paging File", "% Usage", "_Total");
        // Processor Queue Length
        private PerformanceCounter processorQueueLengh = new PerformanceCounter("System", "Processor Queue Length", null);
        private PerformanceCounter handleCountCounter = new PerformanceCounter("Process", "Handle Count", "_Total");
        // 线程数量
        private PerformanceCounter threadCount = new PerformanceCounter("Process", "Thread Count", "_Total");
        // 每秒产生的上下文切换次数
        private PerformanceCounter contentSwitches = new PerformanceCounter("System", "Context Switches/sec", null);
        // 每秒产生的系统调用
        private PerformanceCounter systemCalls = new PerformanceCounter("System", "System Calls/sec", null);

        private PerformanceCounterCategory performanceNetCounterCategory;
        private PerformanceCounter[] trafficSentCounters;
        private PerformanceCounter[] trafficReceivedCounters;
        private string[] interfaces = null;

        public void getProcessorCpuTime()
        {
            float tmp = cpuProcessorTime.NextValue();
            CPUProcessorTime = (float)(Math.Round((double)tmp, 1));
            // Environment.ProcessorCount: return the total number of cores
        }

        public void getCpuPrivilegedTime()
        {
            float tmp = cpuPrivilegedTime.NextValue();
            CPUPrivilegedTime = (float)(Math.Round((double)tmp, 1));
        }

        public void getCpuinterruptTime()
        {
            float tmp = cpuInterruptTime.NextValue();
            CPUInterruptTime = (float)(Math.Round((double)tmp, 1));
        }

        public void getcpuDPCTime()
        {
            float tmp = cpuDPCTime.NextValue();
            CPUDPCTime = (float)(Math.Round((double)tmp, 1));
        }

        public void getPageFile()
        {
            PageFile = pageFile.NextValue();
        }

        public void getProcessorQueueLengh()
        {
            ProcessorQueueLengh = processorQueueLengh.NextValue();
        }

        public void getMemAvailable()
        {
            MEMAvailable = memAvailable.NextValue();
        }

        public void getMemCommited()
        {
            MEMCommited = memCommited.NextValue() / (1024 * 1024);
        }

        public void getMemCommitLimit()
        {
            MEMCommitLimit = memCommitLimit.NextValue() / (1024 * 1024);
        }

        public void getMemCommitedPerc()
        {
            float tmp = memCommitedPerc.NextValue();
            // return the value of Memory Commit Limit
            MEMCommitedPerc = (float)(Math.Round((double)tmp, 1));
        }

        public void getMemPoolPaged()
        {
            float tmp = memPollPaged.NextValue() / (1024 * 1024);
            MEMPoolPaged = (float)(Math.Round((double)tmp, 1));
        }

        public void getMemPoolNonPaged()
        {
            float tmp = memPollNonPaged.NextValue() / (1024 * 1024);
            MEMPoolNonPaged = (float)(Math.Round((double)tmp, 1));
        }

        public void getMemCachedBytes()
        {
            // return the value of Memory Cached in MBytes
            MEMCached = memCached.NextValue() / (1024 * 1024);
        }

        public void getHandleCountCounter()
        {
            HANDLECountCounter = handleCountCounter.NextValue();
        }
        public void getThreadCount()
        {
            THREADCount = threadCount.NextValue();
        }

        public void getContentSwitches()
        {
            CONTENTSwitches = (int)Math.Ceiling(contentSwitches.NextValue());
        }

        public void getsystemCalls()
        {
            SYSTEMCalls = (int)Math.Ceiling(systemCalls.NextValue());
        }

        public void getCurretTrafficSent()
        {
            int length = interfaces.Length;
            float sendSum = 0.0F;
            for (int i = 0; i < length; i++)
            {
                sendSum += trafficSentCounters[i].NextValue();
            }
            float tmp = 8 * (sendSum / 1024);
            NetTrafficSend = (float)(Math.Round((double)tmp, 1));
        }

        public void getSampleTime()
        {
            SamplingTime = DateTime.Now;
        }

        public void initNetCounters()
        {
            // PerformanceCounter(CategoryName,CounterName,InstanceName)
            performanceNetCounterCategory = new PerformanceCounterCategory("Network Interface");
            interfaces = performanceNetCounterCategory.GetInstanceNames();
            int length = interfaces.Length;

            if (length > 0)
            {
                trafficSentCounters = new PerformanceCounter[length];
                trafficReceivedCounters = new PerformanceCounter[length];
            }
            for (int i = 0; i < length; i++)
            {
                // Initializes a new, read-only instance of the PerformanceCounter class.
                //   1st paramenter: "categoryName"-The name of the performance counter category (performance object) with which this performance counter is associated.
                //   2nd paramenter: "CounterName" -The name of the performance counter.
                //   3rd paramenter: "instanceName" -The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.
                trafficReceivedCounters[i] = new PerformanceCounter("Network Interface", "Bytes Sent/sec", interfaces[i]);
                trafficSentCounters[i] = new PerformanceCounter("Network Interface", "Bytes Sent/sec", interfaces[i]);
            }
            // List of all names of the network interfaces
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine("Name netInterface: {0}", performanceNetCounterCategory.GetInstanceNames()[i]);
            }
        }
    }
}
