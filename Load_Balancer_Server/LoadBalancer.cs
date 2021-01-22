/* TODO: add more algorithm for load detect and balancing */
using System;
using System.Collections.Generic;
using System.Text;
using Perf_Transfer;
using System.Threading;
using System.Timers;

/* Load-balancer main function for dynamic loading system in Hyper-V, copyright reserved by chenboyan */
namespace Load_Balancer_Server
{
    public class LoadBalancer
    {
        // 当前用户正在切入使用的虚拟机名
        public string currentUsedVM { set; get; }
        public Dictionary<VirtualMachine, MemoryBalancer> memoryBalancerDict = new Dictionary<VirtualMachine, MemoryBalancer>();
        public Dictionary<VirtualMachine, CpuBalancer> cpuBalancerDict = new Dictionary<VirtualMachine, CpuBalancer>();

        // Host主机的状态信息
        public SystemInfo systemInfo = new SystemInfo();
        public double hostMemReservedPercentage { set; get; }
        // 监测的时间间隔，单位为毫秒(ms)
        int detectTimeGap = 10000;
        // 判断是否进行资源动态调度的时间间隔
        private System.Timers.Timer Balancetimer;

        public DetectorServer myDetectorServer { set; get; }

        public DynamicAdjustment dynamicAdjustment = new DynamicAdjustment();
        public LoadBalancer(List<VirtualMachine> vmList, double memPercentageThredHold, int memAlarmTimesLimit, double percentagethredhold, int queuelengththredhold, int cpuAlarmTimesLimit, int timeGap, double hostMemReserve) 
        {
            detectTimeGap = timeGap;
            hostMemReservedPercentage = hostMemReserve;
            foreach(VirtualMachine vm in vmList)
            {
                // construct balancer for each vm
                MemoryBalancer currentMemoryBalancer = new MemoryBalancer(vm, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuBalancer currentCpuBalancer = new CpuBalancer(vm, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryBalancerDict.Add(vm, currentMemoryBalancer);
                cpuBalancerDict.Add(vm, currentCpuBalancer);
            }
        }

        public void setDetectorServer(DetectorServer detectorServer)
        {
            myDetectorServer = detectorServer;
        }

        public void BalanceByTime()
        {
            // 创建一个detectTimeGap定时的定时器
            Balancetimer = new System.Timers.Timer(detectTimeGap);    // 参数单位为ms
            Balancetimer.Elapsed += BalanceAllVM;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            Balancetimer.AutoReset = true;
            // 使能定时器
            Balancetimer.Enabled = true;
            // 开始计时
            Balancetimer.Start();
        }

        public void BalanceAllVM(object sender, ElapsedEventArgs e)
        {
            // TODO: finish all 
            foreach (KeyValuePair<VirtualMachine, MemoryBalancer> kvp in memoryBalancerDict)
            {
                VirtualMachine currentVM = kvp.Key;
                MemoryBalancer currentMemBalancer = kvp.Value;
                if (currentMemBalancer.isMemAlarm == true)
                {
                    Console.WriteLine("内存预警出现，分配内存");
                    bool ret = dynamicAdjustment.AppendVMMemory(currentVM, 2048, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                    if (ret)
                    {
                        // 分配内存后，取消内存预警并增加内存空闲等级
                        currentMemBalancer.isMemAlarm = false;
                        if (memoryBalancerDict[currentVM].memFreeRanking <= 8)
                            // Memory become more free
                            memoryBalancerDict[currentVM].memFreeRanking += 1;
                    }
                }
            }
            foreach (KeyValuePair<VirtualMachine, CpuBalancer> kvp in cpuBalancerDict)
            {
                VirtualMachine currentVM = kvp.Key;
                CpuBalancer currentCpuBalancer = kvp.Value;
                if (currentCpuBalancer.isCpuAlarm == true)
                {
                    Console.WriteLine("CPU预警出现，分配CPU");
                    ulong currentCpuLimit = currentVM.GetPerformanceSetting().CPU_Limit;
                    if (currentCpuLimit > 90000)
                    {
                        return;
                    }
                    else
                    {
                        bool ret = dynamicAdjustment.AdjustCPULimit(currentVM, currentCpuLimit + 10000);
                        if (ret)
                        {
                            // 取消CPU预警
                            currentCpuBalancer.isCpuAlarm = false;
                            // CPU become more free
                            if (cpuBalancerDict[currentVM].cpuFreeRanking <=8)
                                cpuBalancerDict[currentVM].cpuFreeRanking += 1;
                        }
                    }
                }
            }
        }
        public bool CheckMemCanDeploy()
        {
            //TODO:需要判断能否进行这次内存分配，Host剩余可用内存是否够用，不够用则要尝试压缩其他虚拟机内存
            return true;
        }


        public class MemoryBalancer 
        {
            VirtualMachine currentVirtualMachine { set; get; }
            private double percentageThredHold { set; get; }

            // 当前得到的虚拟机性能计数器值
            public VMPerf currentVMPerf { get => DetectorServer.mySampleServer.vmPerfDict[currentVirtualMachine.vmName]; }

            // 内存告警的次数
            public int detectAlarmTimes { set; get; }
            // 内存告警限制次数，超过该次数isMemAlarm变量置为true
            public int memAlarmTimesLimit { set; get; }
            // 内存空余度的评价等级，1-10，越大越大说明越空闲，回收内存先从空闲度高的虚拟机回收
            public int memFreeRanking { set; get; }
            public int detectTimeGap { set; get; }
            public bool isMemAlarm = false;
            // 内存检测定时器
            private System.Timers.Timer RUtimer;
            // 清空detectAlarmTimes和isMemAlarm的定时器
            private System.Timers.Timer Cleartimer;
            public MemoryBalancer(VirtualMachine vm, double thredhold, int alarmTimesLimit, int detectTime)
            {
                currentVirtualMachine = vm;
                percentageThredHold = thredhold;
                memAlarmTimesLimit = alarmTimesLimit;
                detectTimeGap = detectTime;
                detectAlarmTimes = 0;
                // 设置一个中间值
                memFreeRanking = 5;
            }

            public void SetAlarmTimesZero()
            {
                detectAlarmTimes = 0;
            }

            public void CancelMemAlarm()
            {
                isMemAlarm = false;
            }

            public void DetectMemByTime()
            {
                // 创建一个detectTimeGap定时的定时器
                RUtimer = new System.Timers.Timer(detectTimeGap);    // 参数单位为ms
                RUtimer.Elapsed += DetectMemoryState;
                // 为true时，定时时间到会重新计时；为false则只定时一次
                RUtimer.AutoReset = true;
                // 使能定时器
                RUtimer.Enabled = true;
                // 开始计时
                RUtimer.Start();

                // 创建一个detectTimeGap*100定时的清空状态定时器
                Cleartimer = new System.Timers.Timer(detectTimeGap * 100);    // 参数单位为ms
                Cleartimer.Elapsed += ClearMemState;
                // 为true时，定时时间到会重新计时；为false则只定时一次
                Cleartimer.AutoReset = true;
                // 使能定时器
                Cleartimer.Enabled = true;
                // 开始计时
                Cleartimer.Start();
            }

            public void DetectMemoryState(object sender, ElapsedEventArgs e)
            {
                float memAvailable = currentVMPerf.MEMAvailable;
                float pagesPerSec = currentVMPerf.PagesPerSec;

                double allocMemSize = (double)currentVirtualMachine.performanceSetting.RAM_VirtualQuantity;
                double memPercentage = memAvailable / allocMemSize;
                /* TODO; more details to analysy memory performance */
                if (memPercentage > percentageThredHold)
                {
                    detectAlarmTimes += 1;
                }
                if (detectAlarmTimes > memAlarmTimesLimit)
                {
                    isMemAlarm = true;
                    // 内存空闲等级下降
                    if (memFreeRanking > 1)
                        memFreeRanking -= 1;
                }
            }

            public void ClearMemState(object sender, ElapsedEventArgs e)
            {
                SetAlarmTimesZero();
                CancelMemAlarm();
            }

        }

        public class CpuBalancer
        {
            VirtualMachine currentVirtualMachine { set; get; }
            double percentageThredHold { set; get; }
            int processQueueLengthThredHold { set; get; }

            // 当前得到的虚拟机性能计数器值
            public VMPerf currentVMPerf { get => DetectorServer.mySampleServer.vmPerfDict[currentVirtualMachine.vmName]; }

            // CPU告警的次数
            public int detectAlarmTimes { set; get; }
            // CPU告警限制次数，超过该次数isCpuAlarm变量置为true
            public int cpuAlarmTimesLimit { set; get; }
            // CPU空闲等级
            public int cpuFreeRanking { set; get; }
            public int detectTimeGap { set; get; }
            public bool isCpuAlarm = false;

            
            // CPU检测定时器
            private System.Timers.Timer RUtimer;
            // 清空detectAlarmTimes和isCpuAlarm的定时器
            private System.Timers.Timer Cleartimer;
            public CpuBalancer(VirtualMachine vm, double percentagethredhold, int queuelengththredhold, int alarmTimesLimit, int detectionGap)
            {
                currentVirtualMachine = vm;
                percentageThredHold = percentagethredhold;
                processQueueLengthThredHold = queuelengththredhold;
                detectAlarmTimes = 0;
                cpuAlarmTimesLimit = alarmTimesLimit;
                detectTimeGap = detectionGap;
                cpuFreeRanking = 5;
            }

            public void SetAlarmTimesZero()
            {
                detectAlarmTimes = 0;
            }
            public void CancelCpuAlarm()
            {
                isCpuAlarm = false;
            }

            public void DetectCpuByTime()
            {
                // 创建一个detectTimeGap定时的定时器
                RUtimer = new System.Timers.Timer(detectTimeGap);    // 参数单位为ms
                RUtimer.Elapsed += DetectCpuState;
                // 为true时，定时时间到会重新计时；为false则只定时一次
                RUtimer.AutoReset = true;
                // 使能定时器
                RUtimer.Enabled = true;
                // 开始计时
                RUtimer.Start();

                // 创建一个detectTimeGap*100定时的清空状态定时器
                Cleartimer = new System.Timers.Timer(detectTimeGap * 100);    // 参数单位为ms
                Cleartimer.Elapsed += ClearCpuState;
                // 为true时，定时时间到会重新计时；为false则只定时一次
                Cleartimer.AutoReset = true;
                // 使能定时器
                Cleartimer.Enabled = true;
                // 开始计时
                Cleartimer.Start();
            }
            public void DetectCpuState(object sender, ElapsedEventArgs e)
            {
                float CpuPercentage = currentVMPerf.CPUProcessorTime;
                float processQueueLength = currentVMPerf.ProcessorQueueLength;
                if (CpuPercentage > percentageThredHold || processQueueLength > processQueueLengthThredHold)
                {
                               detectAlarmTimes += 1;
                }
                if (detectAlarmTimes > cpuAlarmTimesLimit)
                {
                    isCpuAlarm = true;
                    if (cpuFreeRanking > 1)
                        cpuFreeRanking -= 1;
                 }
            }
            public void ClearCpuState(object sender, ElapsedEventArgs e)
            {
                SetAlarmTimesZero();
                CancelCpuAlarm();
            }

        }

    }
}
