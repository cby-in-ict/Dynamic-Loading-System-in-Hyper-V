using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Load_Balancer_Server;

/* This proj define the interface between MultiPC and Load-Balancer, MultiPC call SetVMStaus.exe
   to transfer VM Status change to Load-Balancer */

namespace SetVMStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            string vmName = args[0];
            string vmStatus = args[1];
            try
            {
                VMState.SetVMState(vmName, vmStatus);
            }
            catch (Exception exp)
            {
                Console.WriteLine();
            }

        }
    }
}
