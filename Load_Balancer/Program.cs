﻿/*                                ChenBoYan chenboyan@ict.ac.cn
 * This is a Dynamic Balancer for Hyper-V VM, It can ensures a VM has enough free mem to startup,
 * and as the load increase, this system can detect performance shortage and dynamicly adjust the 
 * resource of a VM. for more information, Please refer to Readme.md.
 *                       Copyright preserved by ChenBoYan, chenboyan@ict.ac.cn 
*/

using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Load_Balancer
{
    class Program
    {
        static void Main(string[] args)
        {
            SystemInfo systemInfo = new SystemInfo();
            Int64 AvailableMemory = systemInfo.MemoryAvailable;
            Int64 PhysicalMemory = systemInfo.PhysicalMemory;
            AvailableMemory = AvailableMemory / (1024 * 1024);
            PhysicalMemory = PhysicalMemory / (1024 * 1024);
            int ProcessorCount = systemInfo.ProcessorCount;
            float load = systemInfo.CpuLoad;
            
            Console.WriteLine("CPU总个数：" + ProcessorCount);
            Console.WriteLine("CPU负载率：" + load);
            Console.WriteLine("总物理内存：" + PhysicalMemory + "MB");
            Console.WriteLine("当前可用内存：" + AvailableMemory + "MB");
        }
    }
}