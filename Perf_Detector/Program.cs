/* This is a performance detector in Hyper-V VM, It can transfer performance arguments to Host Load-Balancer 
   which can dynamicly adjust the resource of VM, for more information, Please refer to Readme.md.
                         Copyright preserved by ChenBoYan, chenboyan@ict.ac.cn */

using System;
using System.Threading;

namespace Perf_Detector
{
    class Program
    {
        static void Main(string[] args)
        {
            CpuSpoofer myCpuSpoofer = new CpuSpoofer();
            while (true) 
            {
                Thread.Sleep(1000);
                Console.WriteLine("此时的CPU占用率为：" + Convert.ToString(myCpuSpoofer.CPUProcessorTime));
            }
        }
    }
}
