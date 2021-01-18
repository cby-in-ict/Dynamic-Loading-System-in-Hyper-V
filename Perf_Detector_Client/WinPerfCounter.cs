//#define TEST
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;

/* This class is used to get performance counter, refer to MSDN, copyright partly preserved by cby */
namespace Perf_Detector
{
    public class WinPerfCounter
    {
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
        public float PagesPerSec { get; set; }
        public float PageFaultsPerSec { get; set; }
        public float PageReadsPerSec { get; set; }
        public float PageWritesPerSec { get; set; }
        public float PagesInputPerSec { get; set; }
        public float PagesOutputPerSec { get; set; }

        // 采样时间
        public DateTime SamplingTime { get; set; }
        // CPU占用率
        private PerformanceCounter cpuProcessorTime = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
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
        // Pages/sec
        private PerformanceCounter pagesPerSec = new PerformanceCounter("Memory", "Pages/sec", null);
        // Page Faults/sec
        private PerformanceCounter pageFaultsPerSec = new PerformanceCounter("Memory", "Page Faults/sec", null);
        // Page Reads/sec
        private PerformanceCounter pageReadPerSec = new PerformanceCounter("Memory", "Page Reads/sec", null);
        // Page Writes/sec
        private PerformanceCounter pageWritePerSec = new PerformanceCounter("Memory", "Page Writes/sec", null);
        // Page Inputs/sec
        private PerformanceCounter pageInputPerSec = new PerformanceCounter("Memory", "Pages Input/sec", null);
        // Page Output/sec
        private PerformanceCounter pageOutputPerSec = new PerformanceCounter("Memory", "Pages Output/sec", null);

        private PerformanceCounterCategory performanceNetCounterCategory;
        private PerformanceCounter[] trafficSentCounters;
        private PerformanceCounter[] trafficReceivedCounters;
        private string[] interfaces = null;

        public class QueueFixedLength : Queue
        {
            public QueueFixedLength(int length)
            {
                this.fixedLength = length;
            }
            public QueueFixedLength() : this(10)
            {
            }

            private int fixedLength = 10;
            public int FixedLength
            {
                get
                {
                    return fixedLength;
                }
                set
                {
                    fixedLength = value;
                }
            }

            public override void Enqueue(object obj)
            {
                if (this.Count > 10)
                {
                    base.Dequeue();
                }
                base.Enqueue(obj);
            }
        }
        // 长度为10的队列，记录了历史最近的10次CPU使用率
        QueueFixedLength CPUProcessorTimeQueue = new QueueFixedLength();
        QueueFixedLength MEMAvailableQueue = new QueueFixedLength();

        /* CPU 参数第一次获得的值往往是0，注意! */
        public float getProcessorCpuTime()
        {
            float tmp = cpuProcessorTime.NextValue();
            CPUProcessorTime = (float)(Math.Round((double)tmp, 1));
            // Console.WriteLine("Processor CPU Usage is:" + Convert.ToString(CPUProcessorTime));
            CPUProcessorTimeQueue.Enqueue(CPUProcessorTime);
            return CPUProcessorTime;
        }

        public float getCpuPrivilegedTime()
        {
            float tmp = cpuPrivilegedTime.NextValue();
            CPUPrivilegedTime = (float)(Math.Round((double)tmp, 1));
            return CPUPrivilegedTime;
        }

        public float getCpuinterruptTime()
        {
            float tmp = cpuInterruptTime.NextValue();
            CPUInterruptTime = (float)(Math.Round((double)tmp, 1));
            return CPUInterruptTime;
        }

        public float getcpuDPCTime()
        {
            float tmp = cpuDPCTime.NextValue();
            CPUDPCTime = (float)(Math.Round((double)tmp, 1));
            return CPUDPCTime;
        }

        public float getPageFile()
        {
            PageFile = pageFile.NextValue();
            return PageFile;
        }

        public float getProcessorQueueLengh()
        {
            ProcessorQueueLengh = processorQueueLengh.NextValue();
            return ProcessorQueueLengh;
        }

        public float getMemAvailable()
        {
            MEMAvailable = memAvailable.NextValue();
            MEMAvailableQueue.Enqueue(MEMAvailable);
            return MEMAvailable;
        }

        public float getMemCommited()
        {
            MEMCommited = memCommited.NextValue() / (1024 * 1024);
            return MEMCommited;
        }

        public float getMemCommitLimit()
        {
            MEMCommitLimit = memCommitLimit.NextValue() / (1024 * 1024);
            return MEMCommitLimit;
        }

        public float getMemCommitedPerc()
        {
            float tmp = memCommitedPerc.NextValue();
            // return the value of Memory Commit Limit
            MEMCommitedPerc = (float)(Math.Round((double)tmp, 1));
            return MEMCommitedPerc;
        }

        public float getMemPoolPaged()
        {
            float tmp = memPollPaged.NextValue() / (1024 * 1024);
            MEMPoolPaged = (float)(Math.Round((double)tmp, 1));
            return MEMPoolPaged;
        }

        public float getMemPoolNonPaged()
        {
            float tmp = memPollNonPaged.NextValue() / (1024 * 1024);
            MEMPoolNonPaged = (float)(Math.Round((double)tmp, 1));
            return MEMPoolNonPaged;
        }

        public float getMemCachedBytes()
        {
            // return the value of Memory Cached in MBytes
            MEMCached = memCached.NextValue() / (1024 * 1024);
            return MEMCached;
        }

        public float getHandleCountCounter()
        {
            HANDLECountCounter = handleCountCounter.NextValue();
            return HANDLECountCounter;
        }
        public float getThreadCount()
        {
            THREADCount = threadCount.NextValue();
            return THREADCount;
        }

        public int getContentSwitches()
        {
            CONTENTSwitches = (int)Math.Ceiling(contentSwitches.NextValue());
            return CONTENTSwitches;
        }

        public int getsystemCalls()
        {
            SYSTEMCalls = (int)Math.Ceiling(systemCalls.NextValue());
            return SYSTEMCalls;
        }

        public DateTime getSampleTime()
        {
            SamplingTime = DateTime.Now;
            return SamplingTime;
        }

        //新添加和Page Faults相关的错误
        public float getPageFaultsPerSec()
        {
            PageFaultsPerSec = pageFaultsPerSec.NextValue();
            return PageFaultsPerSec;
        }
        public float getPageReadsPerSec()
        {
            PageReadsPerSec = pageReadPerSec.NextValue();
            return PageReadsPerSec;
        }
        public float getPageWritesPerSec()
        {
            PageWritesPerSec = pageWritePerSec.NextValue();
            return PageWritesPerSec;
        }
        public float getPageInputsPerSec()
        {
            PagesInputPerSec = pageInputPerSec.NextValue();
            return PagesInputPerSec;
        }
        public float getPageOutputsPerSec()
        {
            PagesOutputPerSec = pageOutputPerSec.NextValue();
            return PagesOutputPerSec;
        }
        public float getPagesPerSec()
        {
            PagesPerSec = pagesPerSec.NextValue();
            return PagesPerSec;
        }

        //刷新参数
        public bool initAllCounterValue()
        {
            try
            {
                getProcessorCpuTime();
                getCpuPrivilegedTime();
                getCpuinterruptTime();
                getcpuDPCTime();
                getPageFile();
                getProcessorQueueLengh();
                getMemAvailable();
                getMemCommited();
                getMemCommitLimit();
                getMemCommitedPerc();
                getMemPoolPaged();
                getMemPoolNonPaged();
                getMemCachedBytes();
                getHandleCountCounter();
                getThreadCount();
                getContentSwitches();
                getsystemCalls();
                getSampleTime();
                getPageFaultsPerSec();
                getPageReadsPerSec();
                getPageWritesPerSec();
                getPageInputsPerSec();
                getPageOutputsPerSec();
                getPagesPerSec();
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "更新性能计数器参数异常");
                return false;
            }
        }
        // 单元测试，待添加TODO
#if TEST
        static void Main(string[] args)
        {
            //var counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //Console.WriteLine(Convert.ToString(counter.NextValue()));
            //Thread.Sleep(1000);
            //Console.WriteLine(Convert.ToString(counter.NextValue()));

            var counter = new PerformanceCounter("Memory", "Pages/sec", null);
            Console.WriteLine(Convert.ToString(counter.NextValue()));
            Thread.Sleep(10);
            Console.WriteLine(Convert.ToString(counter.NextValue()));
            
            var counter1 = new PerformanceCounter("Memory", "Pages/sec", null);
            Console.WriteLine(Convert.ToString(counter1.NextValue()));
            Thread.Sleep(1000);
            Console.WriteLine(Convert.ToString(counter1.NextValue()));

            WinPerfCounter winPerfCounter = new WinPerfCounter();
            winPerfCounter.initAllCounterValue();
            Thread.Sleep(1000);
            Console.WriteLine("CPU占用率：" + Convert.ToString(winPerfCounter.getProcessorCpuTime()) + "\nProcessor Queue Length:" + Convert.ToString(winPerfCounter.ProcessorQueueLengh) + "\n可用内存大小：" + Convert.ToString(winPerfCounter.MEMAvailable + "\nPage Faults" + Convert.ToString(winPerfCounter.PageFaultsPerSec) + "\nPages Input" + Convert.ToString(winPerfCounter.PagesInputPerSec)));
        }
#endif
    }

}
