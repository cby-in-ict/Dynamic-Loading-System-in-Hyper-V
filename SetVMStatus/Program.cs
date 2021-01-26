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
            if (args.Length < 2)
                return;
            string vmName = args[1];
            string vmStatus = args[2];
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
