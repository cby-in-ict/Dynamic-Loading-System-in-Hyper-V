/* This is a performance detector in Hyper-V VM, It can transfer performance arguments to Host Load-Balancer 
   which can dynamicly adjust the resource of VM, for more information, Please refer to Readme.md.
                         Copyright preserved by ChenBoYan, chenboyan@ict.ac.cn */

using System;
using System.Threading;

namespace Perf_Detector
{
    class Program_1
    {
        static void Main(string[] args)
        {
            ProcessInfo myProcessInfo = new ProcessInfo();
            CpuSpoofer myCpuSpoofer = new CpuSpoofer();
            MemSpoofer myMEMSpoofer = new MemSpoofer();
            Console.WriteLine(myProcessInfo.getRedirectTaskList());
            //while (true) 
            //{
            //    Thread.Sleep(1000);
            //    Console.WriteLine("此时的可用内存大小为：" + Convert.ToString(myMEMSpoofer.MEMAvailable));
            //    Console.WriteLine("此时的CPU占用率为：" + Convert.ToString(myCpuSpoofer.CPUProcessorTime));
            //}
        }
    }
}
