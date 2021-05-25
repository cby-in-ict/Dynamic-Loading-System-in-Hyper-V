/* TODO: add more algorithm for load detect and balancing */
using System;
using System.Collections.Generic;
using System.Text;
using Perf_Transfer;
using System.Threading;
using System.Timers;
using Microsoft.Windows.ComputeVirtualization;

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
        public int appendMemPressure { set; get; }
        public int recycleMemPressure { set; get; }
        // 监测的时间间隔，单位为毫秒(ms)
        int detectTimeGap = 5000;
        // 是否开启内存和CPU的负载均衡，默认开启
        public bool IsMemoryBalanceOn = true;
        public bool IsCPUBalanceOn = true;
        // 动态均衡的定时器
        private System.Timers.Timer Balancetimer;
        // 接收性能信息的服务器
        public Server myDetectorServer { set; get; }
        // 动态资源调节
        public DynamicAdjustment dynamicAdjustment = new DynamicAdjustment();
        /*List<VirtualMachine> vmList, */
        public LoadBalancer(double memPercentageThredHold, int memAlarmTimesLimit, double percentagethredhold, int queuelengththredhold, int cpuAlarmTimesLimit, int timeGap, double hostMemReserve, int recycleMemThredHold = 57, int appendMemThredHold = 85) 
        {
            detectTimeGap = timeGap;
            hostMemReservedPercentage = hostMemReserve;
            this.recycleMemPressure = recycleMemThredHold;
            this.appendMemPressure = appendMemThredHold;

            // 读取整个系统的配置文件，是否开启内存、CPU负载均衡
            try
            {
                GetConfig getConfig = new GetConfig();
                getConfig.GetSysConfig();
                IsMemoryBalanceOn = getConfig.currentSysConfig.IsMemoryBalanceOn;
                IsCPUBalanceOn = getConfig.currentSysConfig.IsCPUBalanceOn;
            }
            catch 
            {
                IsMemoryBalanceOn = true;
                IsCPUBalanceOn = true;
            }


            if (VMState.VM1 != null)
            {
                MemoryAnalysor currentMemoryBalancer = new MemoryAnalysor(VMState.VM1, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuAnalysor currentCpuBalancer = new CpuAnalysor(VMState.VM1, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryAnalysorDict.Add(VMState.VM1, currentMemoryBalancer);
                cpuAnalysorrDict.Add(VMState.VM1, currentCpuBalancer);
            }
            if (VMState.VM2 != null)
            {
                MemoryAnalysor currentMemoryBalancer = new MemoryAnalysor(VMState.VM2, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuAnalysor currentCpuBalancer = new CpuAnalysor(VMState.VM2, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryAnalysorDict.Add(VMState.VM2, currentMemoryBalancer);
                cpuAnalysorrDict.Add(VMState.VM2, currentCpuBalancer);
            }
            if (VMState.VM3 != null)
            {
                MemoryAnalysor currentMemoryBalancer = new MemoryAnalysor(VMState.VM3, memPercentageThredHold, memAlarmTimesLimit, detectTimeGap);
                currentMemoryBalancer.DetectMemByTime();
                CpuAnalysor currentCpuBalancer = new CpuAnalysor(VMState.VM3, percentagethredhold, queuelengththredhold, cpuAlarmTimesLimit, detectTimeGap);
                currentCpuBalancer.DetectCpuByTime();

                memoryAnalysorDict.Add(VMState.VM3, currentMemoryBalancer);
                cpuAnalysorrDict.Add(VMState.VM3, currentCpuBalancer);
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
                    Console.WriteLine("监测到虚拟机尚未安装");
                    return false;
                }
                if (CPU_Reverve > CPU_Limit)
                {
                    Console.WriteLine("监测到虚拟机保留：" + CPU_Reverve.ToString() + "需要小于虚拟机限制资源：" + CPU_Limit.ToString());
                    return false;
                }

                SystemInfo sysInfo = new SystemInfo();
                long hostAvailableMemory = sysInfo.MemoryAvailable;
                if (PowerOnMemorySize * 1024 * 1.2 > hostAvailableMemory)
                    RequestMem = true;

                DynamicAdjustment adjustment = new DynamicAdjustment();

                bool ret = adjustment.AdjustCPUCount(vm, 4);
                ret &= adjustment.AdjustMemorySize(vm, PowerOnMemorySize);
                ret &= adjustment.AdjustCPUReservation(vm, CPU_Reverve);
                ret &= adjustment.AdjustCPULimit(vm, CPU_Limit);
                ret &= adjustment.AdjustCPUWeight(vm, 10000);

                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine("监测到虚拟机准备开启设置异常：" + exp.Message);
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
                    Console.WriteLine("监测到虚拟机尚未安装");
                    return false;
                }
                if (CPU_Reverve > CPU_Limit)
                {
                    Console.WriteLine("监测到虚拟机保留：" + CPU_Reverve.ToString() + "需要小于虚拟机限制资源：" + CPU_Limit.ToString());
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
                Console.WriteLine("监测到虚拟机准备恢复设置异常：" + exp.Message);
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
            Balancetimer = new System.Timers.Timer(detectTimeGap * 3);    // 参数单位为ms
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
            if (IsMemoryBalanceOn)
            {
                foreach (KeyValuePair<VirtualMachine, MemoryAnalysor> kvp in memoryAnalysorDict)
                {
                    VirtualMachine currentVM = kvp.Key;
                    MemoryAnalysor currentAnalysor = kvp.Value;
                    currentVM.GetPerformanceSetting();
                    // if (currentVM.vmName == "LocalVM" && currentVM.is == VirtualMachine.VirtualMachineStatus.PowerOn)
                    if (VMState.VM1Config != null && currentVM.vmName == VMState.VM1Config.VMName && currentVM.IsPowerOn())
                    {
                        if (VMState.VM1.vmStatus == VirtualMachine.VirtualMachineStatus.RequestPowerOn)
                            continue;

                        if (VMState.VM1PerfCounterInfo == null)
                        {
                            Console.WriteLine("VM1 初始化Hyper-V计数器失败");
                        }
                        VMState.VM1PerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo(VMState.VM1Config.VMName);
                        if (VMState.VM1PerfCounterInfo.currentPressure > appendMemPressure)
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.VM1Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ VM1 Mem] 监测到虚拟机：" + currentVM.vmName + " 内存压力大\n扩展VM1 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 扩展到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                        if (VMState.VM1PerfCounterInfo.availablePercentage < 0.15)
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.VM1Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ VM1 Mem] 监测到虚拟机：" + currentVM.vmName + " 空闲内存不足\n扩展VM1 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 扩展到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                        else if (VMState.VM1PerfCounterInfo.averagePressure < recycleMemPressure)
                        {
                            bool ret = dynamicAdjustment.RecycleVMMemory(currentVM, VMState.VM1Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[- VM1 Mem] 监测到虚拟机：" + currentVM.vmName + " 内存空闲\n回收VM1 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 回收到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                    }
                    // else if (currentVM.vmName == "NetVM1" && currentVM.vmStatus == VirtualMachine.VirtualMachineStatus.PowerOn)
                    else if (VMState.VM2Config != null && currentVM.vmName == VMState.VM2Config.VMName && currentVM.IsPowerOn())
                    {
                        if (VMState.VM2.vmStatus == VirtualMachine.VirtualMachineStatus.RequestPowerOn)
                            continue;

                        VMState.VM2PerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo(VMState.VM2Config.VMName);
                        if (VMState.VM2PerfCounterInfo == null)
                        {
                            Console.WriteLine("VM2 初始化Hyper-V计数器失败");
                            continue;
                        }
                        if (VMState.VM2PerfCounterInfo.currentPressure > appendMemPressure)
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.VM2Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ VM2 Mem] 监测到虚拟机：" + currentVM.vmName + " 内存压力大\n扩展VM2 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 扩展到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                        if (VMState.VM2PerfCounterInfo.availablePercentage < 0.15)
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.VM2Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ VM2 Mem] 监测到虚拟机：" + currentVM.vmName + " 空闲内存不足\n扩展VM2 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 扩展到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                        else if (VMState.VM2PerfCounterInfo.averagePressure < recycleMemPressure)
                        {
                            bool ret = dynamicAdjustment.RecycleVMMemory(currentVM, VMState.VM2Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[- VM2 Mem] 监测到虚拟机：" + currentVM.vmName + " 内存空闲\n回收VM2 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 回收到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                    }
                    else if (VMState.VM3Config != null && currentVM.vmName == VMState.VM3Config.VMName && currentVM.IsPowerOn())
                    {
                        if (VMState.VM3.vmStatus == VirtualMachine.VirtualMachineStatus.RequestPowerOn)
                            continue;

                        VMState.VM3PerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo(VMState.VM3Config.VMName);
                        if (VMState.VM3PerfCounterInfo == null)
                        {
                            Console.WriteLine("VM3 初始化Hyper-V计数器失败");
                            continue;
                        }
                        if (VMState.VM3PerfCounterInfo.currentPressure > appendMemPressure)
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.VM3Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ VM3 Mem] 监测到虚拟机：" + currentVM.vmName + " 内存压力大\n扩展VM3 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB扩展到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                        if (VMState.VM3PerfCounterInfo.availablePercentage < 0.15)
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(currentVM, VMState.VM3Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ VM3 Mem] 监测到虚拟机：" + currentVM.vmName + " 空闲内存不足\n扩展VM3 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB扩展到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                        else if (VMState.VM3PerfCounterInfo.averagePressure < recycleMemPressure)
                        {
                            bool ret = dynamicAdjustment.RecycleVMMemory(currentVM, VMState.VM3Config.MemorySize, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[- VM3 Mem] 监测到虚拟机：" + currentVM.vmName + " 内存空闲\n回收VM3 内存，从:" + Convert.ToString(currentVM.performanceSetting.RAM_VirtualQuantity) + "MB 回收到:" + Convert.ToString(currentVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            continue;
                        }
                    }
                    if (VMState.isDockerInstalled && VMState.isDockerVMPowerOn)
                    {
                        if (VMState.DockerDesktopVM.IsPowerOff())
                            continue;

                        VMState.DockerVMPerfCounterInfo = hyperVPerfCounter.GetVMHyperVPerfInfo("DockerDesktopVM");
                        if (VMState.DockerVMPerfCounterInfo == null)
                        {
                            Console.WriteLine("DockerDesktopVM 初始化Hyper-V计数器失败");
                            continue;
                        }
                        VMState.DockerDesktopVM.GetPerformanceSetting();
                        if (VMState.DockerVMPerfCounterInfo.currentPressure > (2 * 130))
                        {
                            bool ret = dynamicAdjustment.AppendVMMemory(VMState.DockerDesktopVM, VMState.DockerVMConfig.MemorySize, VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[+ Docker Mem]监测到Docker开启，且处于Hyper-V模式，虚拟机：DockerDesktopVM 内存压力大\n扩展内存大小，从:" + Convert.ToString(VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity) + "MB扩展到:" + Convert.ToString(VMState.DockerDesktopVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            else
                            {
                                ulong currentMemSize = VMState.DockerDesktopVM.GetPerformanceSetting().RAM_VirtualQuantity;
                                if (VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity != currentMemSize)
                                    Console.WriteLine("[+ Docker Mem] 监测到Docker开启，且处于Hyper-V模式，虚拟机：DockerDesktopVM 内存压力大\n扩展内存大小，从:" + Convert.ToString(VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity) + "MB扩展到:" + Convert.ToString(currentMemSize) + "MB");
                            }
                            continue;
                        }
                        else if (VMState.DockerVMPerfCounterInfo.averagePressure < (2 * 105))
                        {
                            bool ret = dynamicAdjustment.RecycleVMMemory(VMState.DockerDesktopVM, VMState.DockerVMConfig.MemorySize, VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity, 1);
                            if (ret)
                                Console.WriteLine("[- Docker Mem] 监测到Docker开启，且处于Hyper-V模式，虚拟机：DockerDesktopVM 内存空闲\n回收内存，从:" + Convert.ToString(VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity) + "MB 回收到:" + Convert.ToString(VMState.DockerDesktopVM.GetPerformanceSetting().RAM_VirtualQuantity) + "MB");
                            else
                            {
                                ulong currentMemSize = VMState.DockerDesktopVM.GetPerformanceSetting().RAM_VirtualQuantity;
                                if (VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity != currentMemSize)
                                    Console.WriteLine("[- Docker Mem] 监测到Docker开启，且处于Hyper-V模式，虚拟机：DockerDesktopVM 内存空闲\n回收内存，从:" + Convert.ToString(VMState.DockerDesktopVM.performanceSetting.RAM_VirtualQuantity) + "MB 回收到:" + Convert.ToString(currentMemSize) + "MB");
                            }
                            continue;
                        }
                    }
                    /*
                    if (currentAnalysor.isMemAlarm == true)
                    {
                        bool ret = dynamicAdjustment.AppendVMMemory(currentVM, 2048, currentVM.performanceSetting.RAM_VirtualQuantity, 1);
                        if (ret)
                        {
                            Console.WriteLine("内存预警出现，分配内存");
                            // 分配内存后，刷新虚拟机性能参数
                            currentVM.GetPerformanceSetting();
                            // 分配内存后，取消内存预警并增加内存空闲等级
                            currentAnalysor.isMemAlarm = false;
                            if (memoryAnalysorDict[currentVM].memFreeRanking <= 8)
                                // Memory become more free
                                memoryAnalysorDict[currentVM].memFreeRanking += 1;
                        }
                    }
                    */
                }
            }
            if (IsCPUBalanceOn)
            {
                foreach (KeyValuePair<VirtualMachine, CpuAnalysor> kvp in cpuAnalysorrDict)
                {
                    VirtualMachine currentVM = kvp.Key;
                    CpuAnalysor currentAnalysor = kvp.Value;
                    currentVM.GetPerformanceSetting();
                    ulong currentCpuLimit = currentVM.performanceSetting.CPU_Limit;
                    ulong currentCpuReserve = currentVM.performanceSetting.CPU_Reservation;

                    // isCpuAlarm标记了瞬时的CPU预警，cpuFreeRanking小于等于3说明CPU长期负载较高
                    if (currentAnalysor.isCpuAlarm == true || currentAnalysor.cpuFreeRanking <= 4)
                    {
                        if (currentCpuLimit > 95000)
                        {
                            continue;
                        }
                        else
                        {
                            bool ret;
                            if ((currentCpuLimit + 20000) < 100000)
                            {
                                ret = dynamicAdjustment.AdjustCPULimit(currentVM, currentCpuLimit + 20000);
                            }
                            else
                            {
                                ret = dynamicAdjustment.AdjustCPULimit(currentVM, 100000);
                            }

                            if (currentCpuReserve < 50000)
                            {
                                ret &= dynamicAdjustment.AdjustCPUReservation(currentVM, currentCpuReserve + 10000);
                                Console.WriteLine("[+ " + currentVM.vmName + " CPU] 监测到虚拟机：" + currentVM.vmName + " CPU压力大，CPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.cpuFreeRanking) + "。 分配CPU\n提高CPU保留比到：" + Convert.ToString((currentCpuReserve + 10000) / 1000));
                            }

                            if (ret)
                            {
                                // fix: log显示limit不超过100
                                ulong dst = (currentCpuLimit + 20000) > (ulong)100000 ? 100 : (currentCpuLimit + 20000) / 1000;
                                Console.WriteLine("[+ " + currentVM.vmName + " CPU] 监测到虚拟机：" + currentVM.vmName + " CPU压力大，CPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.cpuFreeRanking) + "。 分配CPU\n提高CPU限制比到：" + Convert.ToString(dst));
                                // 取消CPU预警
                                currentAnalysor.isCpuAlarm = false;
                                // CPU become more free
                                if (cpuAnalysorrDict[currentVM].cpuFreeRanking <= 5)
                                    cpuAnalysorrDict[currentVM].cpuFreeRanking += 1;
                            }
                            continue;
                        }
                    }

                    // 尝试调低CPU保留值
                    if (currentAnalysor.cpuFreeRanking > 6)
                    {
                        // 等级为9和10，同时调低保留和限制
                        if (currentAnalysor.cpuFreeRanking >= 8)
                        {
                            if (currentCpuReserve > 0)
                            {
                                // 首先调低保留值，确保保留<限制
                                bool ret = dynamicAdjustment.AdjustCPUReservation(currentVM, currentCpuReserve - 10000);
                                // 虚拟机保留最低40%
                                if (ret)
                                {
                                    Console.WriteLine("[- " + currentVM.vmName + " CPU] 监测到虚拟机：" + currentVM.vmName + " CPU空闲\nCPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.cpuFreeRanking) + "。 \n调低虚拟机保留到:" + Convert.ToString((currentCpuReserve - 10000) / 1000));
                                }
                            }
                            // 尝试调低CPU限制值
                            if (currentCpuLimit > 50000)
                            {
                                bool ret = dynamicAdjustment.AdjustCPULimit(currentVM, currentCpuLimit - 10000);
                                if (ret)
                                {
                                    Console.WriteLine("[- " + currentVM.vmName + " CPU] 监测到虚拟机：" + currentVM.vmName + " CPU空闲\nCPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.cpuFreeRanking) + "。 \n调低虚拟机限制到:" + Convert.ToString((currentCpuLimit - 10000) / 1000));
                                }
                            }
                            currentAnalysor.cpuFreeRanking -= 2;
                        }
                        // 等级为7、8
                        else if (currentCpuReserve > 0)
                        {
                            bool ret = dynamicAdjustment.AdjustCPUReservation(currentVM, currentCpuReserve - 10000);
                            if (ret)
                            {
                                Console.WriteLine("[- " + currentVM.vmName + " CPU] 监测到虚拟机：" + currentVM.vmName + " CPU空闲\nCPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.cpuFreeRanking) + "。 调低虚拟机保留到:" + Convert.ToString((currentCpuReserve - 10000) / 1000));
                                currentAnalysor.cpuFreeRanking -= 1;
                            }
                        }
                    }

                    // Docker CPU 动态调节
                    if (!VMState.isDockerInstalled)
                        continue;
                    // Docker VM是否安装
                    if (VMState.DockerDesktopVM.IsPowerOn())
                    {
                        VMState.DockerDesktopVM.GetPerformanceSetting();
                        ulong currentDockerCpuLimit = VMState.DockerDesktopVM.performanceSetting.CPU_Limit;
                        ulong currentDockerCpuReserve = VMState.DockerDesktopVM.performanceSetting.CPU_Reservation;

                        if (currentAnalysor.isDockerCpuAlarm == true)
                        {
                            if (currentDockerCpuLimit > 90000)
                            {
                                continue;
                            }
                            else
                            {
                                bool ret = dynamicAdjustment.AdjustCPULimit(VMState.DockerDesktopVM, currentDockerCpuLimit + 10000);
                                if (currentDockerCpuReserve < 50000)
                                {
                                    ret &= dynamicAdjustment.AdjustCPUReservation(VMState.DockerDesktopVM, currentDockerCpuReserve + 10000);
                                    Console.WriteLine("[+ Docker CPU] 监测到DockerDesktopVM虚拟机 CPU压力大，CPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.dockerVmCpuFreeRanking) + "。 分配CPU\n提高CPU保留比到：" + Convert.ToString((currentDockerCpuReserve + 10000) / 1000));
                                }

                                if (ret)
                                {
                                    Console.WriteLine("[+ Docker CPU] 监测到虚拟机DockerDesktopVM虚拟机 CPU压力大，CPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.dockerVmCpuFreeRanking) + "。 分配CPU\n提高CPU限制比到：" + Convert.ToString((currentDockerCpuLimit + 10000) / 1000));
                                    // 取消CPU预警
                                    currentAnalysor.isCpuAlarm = false;
                                    // CPU become more free。调高CPU空闲等级
                                    if (currentAnalysor.dockerVmCpuFreeRanking <= 5)
                                        currentAnalysor.dockerVmCpuFreeRanking += 1;
                                    continue;
                                }
                            }
                        }

                        // 尝试调低CPU保留值
                        if (currentAnalysor.dockerVmCpuFreeRanking > 6)
                        {
                            // 等级为9和10，同时调低保留和限制
                            if (currentAnalysor.dockerVmCpuFreeRanking > 8)
                            {
                                if (currentDockerCpuReserve > 0)
                                {
                                    // 首先调低保留值，确保保留<限制
                                    bool ret = dynamicAdjustment.AdjustCPUReservation(VMState.DockerDesktopVM, currentDockerCpuReserve - 10000);
                                    // 虚拟机保留最低40%
                                    if (ret)
                                    {
                                        Console.WriteLine("[- Docker CPU] 监测到DockerDesktopVM虚拟机 CPU空闲\nCPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.dockerVmCpuFreeRanking) + "。 \n调低虚拟机保留到:" + Convert.ToString((currentDockerCpuReserve - 10000) / 1000));
                                    }
                                }
                                // 尝试调低CPU限制值
                                if (currentDockerCpuLimit > 40000)
                                {
                                    bool ret = dynamicAdjustment.AdjustCPULimit(VMState.DockerDesktopVM, currentDockerCpuLimit - 10000);
                                    if (ret)
                                    {
                                        Console.WriteLine("[- Docker CPU] 监测到DockerDesktopVM虚拟机 CPU空闲\nCPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.dockerVmCpuFreeRanking) + "。 \n调低虚拟机限制到:" + Convert.ToString((currentDockerCpuLimit - 10000) / 1000));
                                    }
                                }
                                currentAnalysor.dockerVmCpuFreeRanking -= 2;
                            }
                            // 等级为7、8
                            else if (currentDockerCpuReserve > 0)
                            {
                                bool ret = dynamicAdjustment.AdjustCPUReservation(VMState.DockerDesktopVM, currentDockerCpuReserve - 10000);
                                if (ret)
                                {
                                    Console.WriteLine("[- Docker CPU] 监测到DockerDesktopVM虚拟机 CPU空闲\nCPUFreeRanking 等级为：" + Convert.ToString(currentAnalysor.dockerVmCpuFreeRanking) + "。 调低虚拟机保留到:" + Convert.ToString((currentDockerCpuReserve - 10000) / 1000));
                                    currentAnalysor.dockerVmCpuFreeRanking -= 1;
                                }
                            }
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
                return;
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
            double busyPercentageThredHold { set; get; }
            double freePercThredHold { set; get; }
            int processQueueLengthThredHold { set; get; }

            // 当前得到的虚拟机性能计数器值
            public VMPerf currentVMPerf { set; get; }

            // CPU告警的次数
            public int detectAlarmTimes { set; get; }
            // CPU告警限制次数，超过该次数isCpuAlarm变量置为true
            public int cpuAlarmTimesLimit { set; get; }
            // CPU空闲等级CPU空闲等级
            public int cpuFreeRanking { set; get; }
            // DockerDesktopVM
            public int dockerVmCpuFreeRanking { set; get; }
            public int detectTimeGap { set; get; }
            public bool isCpuAlarm = false;

            // Docker虚拟机CPU告警信号
            public bool isDockerCpuAlarm = false;

            // CPU检测定时器
            private System.Timers.Timer RUtimer;
            // 清空detectAlarmTimes和isCpuAlarm的定时器
            private System.Timers.Timer Cleartimer;
            public CpuAnalysor(VirtualMachine vm, double busypercentagethredhold, int queuelengththredhold, int alarmTimesLimit, int detectionGap, double freepercentagethredhold = 30.0)
            {
                currentVirtualMachine = vm;
                busyPercentageThredHold = busypercentagethredhold;
                processQueueLengthThredHold = queuelengththredhold;
                detectAlarmTimes = 0;
                cpuAlarmTimesLimit = alarmTimesLimit;
                detectTimeGap = detectionGap;
                cpuFreeRanking = 5;
                dockerVmCpuFreeRanking = 5;
                freePercThredHold = freepercentagethredhold;
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

                // 创建一个detectTimeGap*1000定时的清空状态定时器
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
                // Detect Docker VM
                if (VMState.DockerDesktopVM != null && VMState.DockerDesktopVM.IsPowerOn()) 
                {
                    float dockerVMRatio = VMState.DockerDesktopVM.GetPerformanceSetting().GuestCpuRatio;
                    ushort[] dockerVMLoadHistory = VMState.DockerDesktopVM.GetPerformanceSetting().ProcessorLoadHistory;
         
                    if (dockerVMLoadHistory != null && dockerVMLoadHistory.Length > 90)
                    {
                        bool isDockerCpuFree = true;
                        int freeCountSample = 0;
                        foreach (UInt16 percentage in dockerVMLoadHistory)
                        {
                            // 换算成占总物理CPU的百分比
                            // 判断CPU是否紧张
                            if (percentage > dockerVMRatio * busyPercentageThredHold)
                            {
                                detectAlarmTimes += 1;
                            }
                            if (detectAlarmTimes > 60)
                            {
                                isDockerCpuAlarm = true;
                                if (dockerVmCpuFreeRanking > 1)
                                    dockerVmCpuFreeRanking -= 1;
                            }

                            // 计算空闲的采样数
                            if (percentage < dockerVMRatio * freePercThredHold)
                            {
                                freeCountSample += 1;
                            }
                        }
                        if (freeCountSample < 85)
                            isDockerCpuFree = false;
                        if (isDockerCpuFree && dockerVmCpuFreeRanking <= 9)
                            dockerVmCpuFreeRanking += 1;
                    }
                }

                // if not PowerOn, do nothing
                if (currentVirtualMachine.IsPowerOff())
                    return;
                if (!Server.mySampleServer.vmPerfDict.ContainsKey(currentVirtualMachine.vmName))
                {
                    ushort[] processorLoadHistory = currentVirtualMachine.GetPerformanceSetting().ProcessorLoadHistory;
                    float guestCpuRatio = currentVirtualMachine.GetPerformanceSetting().GuestCpuRatio;

                    if (processorLoadHistory == null)
                        return;
                    if (processorLoadHistory.Length > 90)
                    {
                        bool isCpuFree = false;
                        bool isCpuBusy = false;
                        int freeCountSample = 0;
                        int busyCountSample = 0;
                        foreach (UInt16 percentage in processorLoadHistory)
                        {
                            // 换算成占总物理CPU的百分比
                            if (percentage < guestCpuRatio * freePercThredHold)
                            {
                                freeCountSample += 1;
                            }
                            if (percentage > guestCpuRatio * busyPercentageThredHold)
                            {
                                busyCountSample += 1;
                            }
                        }

                        if (freeCountSample > 85)
                            isCpuFree = true;
                        if (busyCountSample > 80)
                        {
                            isCpuBusy = true;
                            isCpuFree = false;
                        }
                        // 若CPU空闲，增大空闲评级
                        if (isCpuFree && cpuFreeRanking <= 9)
                            cpuFreeRanking += 1;
                        // 若CPU繁忙，减小空闲评级
                        if (isCpuBusy && cpuFreeRanking >= 2)
                            cpuFreeRanking -= 1;
                    }
                }
                else 
                {
                    currentVMPerf = Server.mySampleServer.vmPerfDict[currentVirtualMachine.vmName];
                    float processQueueLength = currentVMPerf.ProcessorQueueLength;
                    float currentCpuPercentage = currentVMPerf.CPUProcessorTime;
                    if (currentCpuPercentage > 90)
                    {
                        detectAlarmTimes += 1;
                    }
                    // 通过processQueueLength计数器响应瞬时的CPU压力
                    if (processQueueLength > processQueueLengthThredHold)
                    {
                        detectAlarmTimes += 1;
                    }
                    if (detectAlarmTimes > cpuAlarmTimesLimit)
                    {
                        isCpuAlarm = true;
                        if (cpuFreeRanking > 1)
                        {
                            cpuFreeRanking -= 1;
                            return;
                        }
                    }
                    // 监测瞬时压力没有返回，则判断历史使用信息
                    ushort[] processorLoadHistory = currentVirtualMachine.GetPerformanceSetting().ProcessorLoadHistory;
                    float guestCpuRatio = currentVirtualMachine.GetPerformanceSetting().GuestCpuRatio;
                    if (processorLoadHistory == null)
                        return;
                    if (processorLoadHistory.Length > 90)
                    {
                        bool isCpuFree = false;
                        bool isCpuBusy = false;
                        int freeCountSample = 0;
                        int busyCountSample = 0;
                        foreach (UInt16 percentage in processorLoadHistory)
                        {
                            // 换算成占总物理CPU的百分比
                            if (percentage < guestCpuRatio * freePercThredHold)
                            {
                                freeCountSample += 1;
                            }
                            if (percentage > guestCpuRatio * busyPercentageThredHold)
                            {
                                busyCountSample += 1;
                            }
                        }

                        if (freeCountSample > 85)
                            isCpuFree = true;
                        if (busyCountSample > 80)
                        {
                            isCpuBusy = true;
                            isCpuFree = false;
                        }
                        // 若CPU空闲，增大空闲评级
                        if (isCpuFree && cpuFreeRanking <= 9)
                            cpuFreeRanking += 1;
                        // 若CPU繁忙，减小空闲评级
                        if (isCpuBusy && cpuFreeRanking >= 2)
                            cpuFreeRanking -= 1;
                    }
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
