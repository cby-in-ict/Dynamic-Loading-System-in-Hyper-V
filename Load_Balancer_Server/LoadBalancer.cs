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
        // Dictionary<vm, Spoofer>,虚拟机及其对应的监测器
        public Dictionary<VirtualMachine, MemoryAnalysor> memoryAnalysorDict = new Dictionary<VirtualMachine, MemoryAnalysor>();
        public Dictionary<VirtualMachine, CpuAnalysor> cpuAnalysorrDict = new Dictionary<VirtualMachine, CpuAnalysor>();

        // Host主机的状态信息
        public SystemInfo systemInfo = new SystemInfo();
        // Hyper-V 性能计数器
        public HyperVPerfCounter hyperVPerfCounter = new HyperVPerfCounter();
        public double hostMemReservedPercentage { set; get; }
        // 监测的时间间隔，单位为毫秒(ms)
        int detectTimeGap = 10000;
        // 动态均衡的定时器
        private System.Timers.Timer Balancetimer;
        // 接收性能信息的服务器
        public Server myDetectorServer { set; get; }
        // 动态资源调节
        public DynamicAdjustment dynamicAdjustment = new DynamicAdjustment();
        /*List<VirtualMachine> vmList, */
        public LoadBalancer(double memPercentageThredHold, int memAlarmTimesLimit, double percentagethredhold, int queuelengththredhold, int cpuAlarmTimesLimit, int timeGap, double hostMemReserve) 
        {
            detectTimeGap = timeGap;
            hostMemReservedPercentage = hostMemReserve;

            if (VMState.LocalVM != null)
            {
                MemoryAnalysor currentMemoryBalancer = new MemoryAnalysor(VMState.LocalVM, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuAnalysor currentCpuBalancer = new CpuAnalysor(VMState.LocalVM, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryAnalysorDict.Add(VMState.LocalVM, currentMemoryBalancer);
                cpuAnalysorrDict.Add(VMState.LocalVM, currentCpuBalancer);
            }
            if (VMState.NetVM1 != null)
            {
                MemoryAnalysor currentMemoryBalancer = new MemoryAnalysor(VMState.NetVM1, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuAnalysor currentCpuBalancer = new CpuAnalysor(VMState.NetVM1, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryAnalysorDict.Add(VMState.NetVM1, currentMemoryBalancer);
                cpuAnalysorrDict.Add(VMState.NetVM1, currentCpuBalancer);
            }
            if (VMState.NetVM2 != null)
            {
                MemoryAnalysor currentMemoryBalancer = new MemoryAnalysor(VMState.NetVM2, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuAnalysor currentCpuBalancer = new CpuAnalysor(VMState.NetVM2, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryAnalysorDict.Add(VMState.NetVM2, currentMemoryBalancer);
                cpuAnalysorrDict.Add(VMState.NetVM2, currentCpuBalancer);
            }

        }

        // 静态方法，供VMState类直接调用，请求开机时为虚拟机超量分配资源
        public static bool PreparePowerOnVM(VirtualMachine vm, UInt64 PowerOnMemorySize, out bool RequestMem, UInt64 CPU_Reverve = 50000, UInt64 CPU_Limit = 100000)
        {
            try
            {
                RequestMem = false;
                if (vm == null)
                {
                    Console.WriteLine("虚拟机尚未安装");
                    return false;
                }
                if (CPU_Reverve > CPU_Limit)
                {
                    Console.WriteLine("虚拟机保留：" + CPU_Reverve.ToString() + "需要小于虚拟机限制资源：" + CPU_Limit.ToString());
                    return false;
                }

                SystemInfo sysInfo = new SystemInfo();
                long hostAvailableMemory = sysInfo.MemoryAvailable;
                if (PowerOnMemorySize * 1024 * 1.2 > hostAvailableMemory)
                    RequestMem = true;

                DynamicAdjustment adjustment = new DynamicAdjustment();

                bool ret = adjustment.AdjustCPUCount(vm, 4);
                ret &= adjustment.AdjustCPUReservation(vm, CPU_Reverve);
                ret &= adjustment.AdjustCPULimit(vm, CPU_Limit);
                ret &= adjustment.AdjustCPUWeight(vm, 10000);

                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine("虚拟机准备开启设置异常：" + exp.Message);
                RequestMem = false;
                return false;
            }
        }

        public static bool ResumePowerOnVM(VirtualMachine vm, UInt64 PowerOnMemorySize, UInt64 CPU_Reverve = 50000, UInt64 CPU_Limit = 50000)
        {
            try
            {
                if (vm == null)
                {
                    Console.WriteLine("虚拟机尚未安装");
                    return false;
                }
                if (CPU_Reverve > CPU_Limit)
                {
                    Console.WriteLine("虚拟机保留：" + CPU_Reverve.ToString() + "需要小于虚拟机限制资源：" + CPU_Limit.ToString());
                    return false;
                }

                DynamicAdjustment adjustment = new DynamicAdjustment();

                bool ret = adjustment.AdjustCPUReservation(vm, CPU_Reverve);
                ret &= adjustment.AdjustCPULimit(vm, CPU_Limit);
                ret &= adjustment.AdjustMemorySize(vm, PowerOnMemorySize);
                ret &= adjustment.AdjustCPUWeight(vm, 100);

                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine("虚拟机准备恢复设置异常：" + exp.Message);
                return false;
            }
        }

        public void setDetectorServer(Server detectorServer)
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
            foreach (KeyValuePair<VirtualMachine, MemoryAnalysor> kvp in memoryAnalysorDict)
            {
                VirtualMachine currentVM = kvp.Key;
                MemoryAnalysor currentMemBalancer = kvp.Value;
                if (currentVM.vmName == "LocalVM" && currentVM.vmStatus == VirtualMachine.VirtualMachineStatus.PowerOn)
                {
                    if (VMState.LocalVM.vmStatus == VirtualMachine.VirtualMachineStatus.RequestPowerOn)
                        continue;

                    VMState.LocalVMPerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo("LocalVM");
                    currentVM.GetPerformanceSetting();
                    if (VMState.LocalVMPerfCounterInfo.currentPressure > 100)
                    {
                        bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.LocalVMConfig.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                        continue;
                    }
                    else if (VMState.LocalVMPerfCounterInfo.averagePressure < 65)
                    {
                        bool ret = dynamicAdjustment.RecycleVMMemory(currentVM, VMState.LocalVMConfig.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);

                        continue;
                    }
                }
                else if (currentVM.vmName == "NetVM1" && currentVM.vmStatus == VirtualMachine.VirtualMachineStatus.PowerOn)
                {
                    if (VMState.NetVM1.vmStatus == VirtualMachine.VirtualMachineStatus.RequestPowerOn)
                        continue;

                    VMState.NetVM1PerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo("NetVM1");
                    currentVM.GetPerformanceSetting();
                    if (VMState.NetVM1PerfCounterInfo.currentPressure > 100)
                    {
                        bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.NetVM1Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                        continue;
                    }
                    else if (VMState.NetVM1PerfCounterInfo.averagePressure < 65)
                    {
                        bool ret = dynamicAdjustment.RecycleVMMemory(currentVM, VMState.NetVM1Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                        continue;
                    }
                }
                else if (currentVM.vmName == "NetVM2" && currentVM.vmStatus == VirtualMachine.VirtualMachineStatus.PowerOn)
                {
                    if(VMState.NetVM2.vmStatus == VirtualMachine.VirtualMachineStatus.RequestPowerOn)
                        continue;
                   
                    VMState.NetVM2PerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo("NetVM2");
                    currentVM.GetPerformanceSetting();
                    if (VMState.NetVM2PerfCounterInfo.currentPressure > 100)
                    {
                        bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.NetVM2Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                        continue;
                    }
                    else if (VMState.NetVM2PerfCounterInfo.averagePressure < 65)
                    {
                        bool ret = dynamicAdjustment.RecycleVMMemory(currentVM, VMState.NetVM2Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                        continue;
                    }
                }
                if (currentMemBalancer.isMemAlarm == true)
                {
                    Console.WriteLine("内存预警出现，分配内存");
                    bool ret = dynamicAdjustment.AppendVMMemory(currentVM, 2048, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                    if (ret)
                    {
                        // 分配内存后，刷新虚拟机性能参数
                        currentVM.GetPerformanceSetting();
                        // 分配内存后，取消内存预警并增加内存空闲等级
                        currentMemBalancer.isMemAlarm = false;
                        if (memoryAnalysorDict[currentVM].memFreeRanking <= 8)
                            // Memory become more free
                            memoryAnalysorDict[currentVM].memFreeRanking += 1;
                    }
                }
            }
            foreach (KeyValuePair<VirtualMachine, CpuAnalysor> kvp in cpuAnalysorrDict)
            {
                VirtualMachine currentVM = kvp.Key;
                CpuAnalysor currentCpuBalancer = kvp.Value;
                if (currentCpuBalancer.isCpuAlarm == true)
                {
                    Console.WriteLine("CPU预警出现，分配CPU");
                    ulong currentCpuLimit = currentVM.GetPerformanceSetting().CPU_Limit;
                    if (currentCpuLimit > 90000)
                    {
                        continue;
                    }
                    else
                    {
                        bool ret = dynamicAdjustment.AdjustCPULimit(currentVM, currentCpuLimit + 10000);
                        if (ret)
                        {
                            currentVM.GetPerformanceSetting();
                            // 取消CPU预警
                            currentCpuBalancer.isCpuAlarm = false;
                            // CPU become more free
                            if (cpuAnalysorrDict[currentVM].cpuFreeRanking <=8)
                                cpuAnalysorrDict[currentVM].cpuFreeRanking += 1;
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


        public class MemoryAnalysor 
        {
            VirtualMachine currentVirtualMachine { set; get; }
            private double percentageThredHold { set; get; }

            // 当前得到的域内虚拟机性能信息
            public VMPerf currentVMPerf { set; get; }

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
            public MemoryAnalysor(VirtualMachine vm, double thredhold, int alarmTimesLimit, int detectTime)
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

                // if not PowerOn, do nothing.
                if (currentVirtualMachine.vmStatus != VirtualMachine.VirtualMachineStatus.PowerOn)
                    return;
                if (!Server.mySampleServer.vmPerfDict.ContainsKey(currentVirtualMachine.vmName))
                {
                    return;
                }
                currentVMPerf = Server.mySampleServer.vmPerfDict[currentVirtualMachine.vmName];
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

        public class CpuAnalysor
        {
            VirtualMachine currentVirtualMachine { set; get; }
            double percentageThredHold { set; get; }
            int processQueueLengthThredHold { set; get; }

            // 当前得到的虚拟机性能计数器值
            public VMPerf currentVMPerf { set; get; }

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
            public CpuAnalysor(VirtualMachine vm, double percentagethredhold, int queuelengththredhold, int alarmTimesLimit, int detectionGap)
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
                // if not PowerOn, do nothing
                if (currentVirtualMachine.vmStatus != VirtualMachine.VirtualMachineStatus.PowerOn)
                    return;
                if (!Server.mySampleServer.vmPerfDict.ContainsKey(currentVirtualMachine.vmName))
                {
                    return;
                }
                currentVMPerf = Server.mySampleServer.vmPerfDict[currentVirtualMachine.vmName];
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
