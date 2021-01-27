using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* This proj define the interface between MultiPC and Load-Balancer, MultiPC call SetVMStaus.exe
   to transfer VM Status change to Load-Balancer */

namespace SetVMStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                return;
            string action = args[0];
            string vmName = args[1];
            string vmStatus = args[2];

            if (action.Contains("GetIn"))
            {
                try
                {
                    MemoryMapping memoryMapping = new MemoryMapping("vmStatus");
                    memoryMapping.WriteString("GetIn" + "#" + vmName);

                    Console.WriteLine("GetIn VirtualMachine:" + vmName);
                }
                catch (Exception exp)
                {
                    Console.WriteLine();
                }
            }
            else if (action.Contains("GetOff"))
            {
                try
                {
                    MemoryMapping memoryMapping = new MemoryMapping("vmStatus");
                    memoryMapping.WriteString("GetOff" + "#" + vmName);

                    Console.WriteLine("GetOff VirtualMachine:" + vmName);
                }
                catch (Exception exp)
                {
                    Console.WriteLine();
                }
            }
            else
            {
                try
                {
                    MemoryMapping memoryMapping = new MemoryMapping("vmStatus");
                    memoryMapping.WriteString(vmName + "#" + vmStatus);

                    Console.WriteLine("VMName is:" + vmName + "VMStatus is:" + vmStatus);
                }
                catch (Exception exp)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}
