using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perf_Detector_Client
{
    class Program
    {
        static void RecursiveStartClient(string HvAddr)
        {
            try
            {
                Client client = new Client(HvAddr);
                client.StartUpClient();
                
                ProcessControler processControler = new ProcessControler(@"C:\TEMP\ProcConfig.json", 5000);
                processControler.StartUpCheckingProcess(client);
                Console.WriteLine("性能监测器(HvMonitor)成功启动");
                while (true)
                {
                    string input = Console.ReadLine();
                    if (input == "s")
                    {
                        return;
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exp in Client：" + exp.Message);
                Console.WriteLine("Trying to restart client...");
                Thread.Sleep(5000);
                RecursiveStartClient(HvAddr);
            }
        }
        static void Main(string[] args)
        {

            string HvAddr = "hypervnb://a42e7cda-d03f-480c-9cc2-a4de20abb878/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            
            RecursiveStartClient(HvAddr);
        }
    }
}
