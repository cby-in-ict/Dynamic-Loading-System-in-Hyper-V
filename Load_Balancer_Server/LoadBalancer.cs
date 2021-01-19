/* */
using System;
using System.Collections.Generic;
using System.Text;
using Perf_Transfer;

/* Unit test for dynamic loading system in Hyper-V, copyright reserved by chenboyan */
namespace Load_Balancer_Server
{
    public class LoadBalancer
    {
        public MemoryBalancer memoryBalancer { set; get; }
        public CpuBalancer cpuBalancer { set; get; }
        // 监测的时间间隔，单位为毫秒(ms)
        int detectTimeGap = 10000;
        public LoadBalancer(VirtualMachine vm, double memPercentageThredHold, double percentagethredhold, int queuelengththredhold, int timeGap) 
        {
            memoryBalancer = new MemoryBalancer(vm, memPercentageThredHold);
            cpuBalancer = new CpuBalancer(vm, percentagethredhold, queuelengththredhold);
            detectTimeGap = timeGap;
        }

        public void DetectByTime()
        {

        }

        public class MemoryBalancer 
        {
            VirtualMachine currentVirtualMachine;
            double thredHold { set; get; }

            // 当前得到的虚拟机性能计数器值
            public VMPerf currentVMPerf { set; get; }

            // 内存告警的次数
            public int detectAlarmTimes { set; get; }
            // 内存告警限制次数，超过该次数isMemAlarm变量置为true
            int memAlarmTimesLimit { set; get; }
            bool isMemAlarm = false;
            public MemoryBalancer(VirtualMachine vm, double thredhold)
            {
                currentVirtualMachine = vm;
                thredHold = thredhold;
                detectAlarmTimes = 0;
            }

            public void SetVMPerf(VMPerf vmPerf)
            {
                currentVMPerf = vmPerf;
            }

            public void DetectMemoryState()
            {
                float memAvailable = currentVMPerf.MEMAvailable;
                float pagesPerSec = currentVMPerf.PagesPerSec;

                double allocMemSize = (double)currentVirtualMachine.performanceSetting.RAM_VirtualQuantity;
                double memPercentage = memAvailable / allocMemSize;
                if (memPercentage > thredHold)
                {
                    detectAlarmTimes += 1;
                }
                if (detectAlarmTimes > memAlarmTimesLimit)
                    isMemAlarm = true;
            }

            public void SetAlarmTimesZero()
            {
                detectAlarmTimes = 0;
            }
        }

        public class CpuBalancer
        {
            VirtualMachine currentVirtualMachine;
            double percentageThredHold { set; get; }
            int processQueueLengthThredHold { set; get; }

            // 当前得到的虚拟机性能计数器值
            public VMPerf currentVMPerf { set; get; }

            // CPU告警的次数
            public int detectAlarmTimes { set; get; }
            public CpuBalancer(VirtualMachine vm, double percentagethredhold, int queuelengththredhold)
            {
                currentVirtualMachine = vm;
                percentageThredHold = percentagethredhold;
                processQueueLengthThredHold = queuelengththredhold;
                detectAlarmTimes = 0;
            }

            public void SetVMPerf(VMPerf vmPerf)
            {
                currentVMPerf = vmPerf;
            }

            public void DetectCpuState()
            {
                float CpuPercentage = currentVMPerf.CPUProcessorTime;
                float processQueueLength = currentVMPerf.ProcessorQueueLength;
                if (CpuPercentage > percentageThredHold || processQueueLength > processQueueLengthThredHold)
                {
                    detectAlarmTimes += 1;
                }
            }

            public void SetAlarmTimesZero()
            {
                detectAlarmTimes = 0;
            }
        }

    }
}
