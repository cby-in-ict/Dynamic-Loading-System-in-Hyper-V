/*                                ChenBoYan chenboyan@ict.ac.cn
 * This is a Dynamic Balancer for Hyper-V VM, It can ensures a VM has enough free mem to startup,
 * and as the load increase, this system can detect performance shortage and dynamicly adjust the 
 * resource of a VM. for more information, Please refer to Readme.md.
 *                       Copyright preserved by ChenBoYan, chenboyan@ict.ac.cn 
*/
//#define TEST
using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Samples.HyperV.Common;
using System.IO;
using Perf_Transfer;
using System.Threading;

namespace Load_Balancer_Server
{
    class Program
    {
#if TEST

#else
        static void Main(string[] args)
        {
#if MPC
            string MpcVmConfigpath = @"C:\Users\CBY\Documents\代码库\0106\FieldManagerUI\FieldManagerUI\bin\UsefulFile\VMState.json";
            if (!File.Exists(MpcVmConfigpath))
            {
                Console.WriteLine("路径" + MpcVmConfigpath + "下，多域PC虚拟机配置文件不存在");
                return;
            }
            VMState vMState = new VMState(MpcVmConfigpath);
            vMState.StartMessageReceiver();
#else
            VMState vMState = new VMState();
            vMState.StartDetectVMState(5000);
#endif
            InitVMSetting initVMSetting = new InitVMSetting();
            initVMSetting.InitAllVM();

            string HvAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            Server detectorServer = new Server(HvAddr);
            var task = new Task(() =>
            {
                detectorServer.StartUpServer();
            });
            task.Start();

            LoadBalancer testLoadBalancer = new LoadBalancer(80.0, 1, 85.0, 3, 3, 5000, 200000);
            testLoadBalancer.setDetectorServer(detectorServer);
            testLoadBalancer.BalanceByTime();
            Console.WriteLine("负载均衡器(HvBalancer)启动成功！");
            // 按s键退出
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "s")
                {
                    return;
                }
            }
        }
#endif
        }
}
