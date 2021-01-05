/* 
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
            HostState mySysState = new HostState();
            Int64 memSize =  mySysState.getAvailableMemory();
            Console.WriteLine("当前内存剩余大小：" + memSize);
        }
    }
}
