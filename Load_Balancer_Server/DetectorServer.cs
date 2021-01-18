using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Perf_Transfer;

namespace Load_Balancer_Server
{
    public class DetectorServer
    {
        public string DetectorServerAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
        public static VMPerf currentPerfTransfer;
        public DetectorServer(string Addr)
        {
            DetectorServerAddr = Addr;
        }

        [ServiceContract]
        public interface IServer
        {
            [OperationContract]
            bool TransferPerfAnalysis(Perf_Analysis perf_Detector);
            [OperationContract]
            VMPerf TransferPerfStr(string perf_Str);
            [OperationContract]
            bool TransferPerf(VMPerf perf_Transfer);
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
        class SampleServer : IServer
        {
            public bool TransferPerfAnalysis(Perf_Analysis perf_Detector)
            {
                Console.WriteLine($"Received {perf_Detector.CPU_K}");
                return true;
            }
            public VMPerf TransferPerfStr(string perf_Str)
            {
                Console.WriteLine($"Received {perf_Str}");
#if Debug
                Console.WriteLine("收到性能特征字符串");
#endif
                VMPerf perf_Transfer;
                byte[] buffer = Convert.FromBase64String(perf_Str);
                MemoryStream stream = new MemoryStream(buffer);
                BinaryFormatter formatter = new BinaryFormatter();
                perf_Transfer = (VMPerf)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
                // TODO: 回调函数，处理perf_Transfer
                DetectorServer.currentPerfTransfer = perf_Transfer;

                return perf_Transfer;
            }
            public bool TransferPerf(VMPerf perf_Transfer)
            {
                Console.WriteLine("收到了性能信息，其中可用内存大小为：" + perf_Transfer.MEMAvailable);
                DetectorServer.currentPerfTransfer = perf_Transfer;
                return true;
            }
        }
        public bool StartUpServer()
        {
            try
            {   
                var sh = new ServiceHost(new SampleServer());
                var binding = new HyperVNetBinding();
                sh.AddServiceEndpoint(typeof(IServer), binding, DetectorServerAddr);
                sh.Open();
                Console.ReadLine();
                sh.Close();
                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "启动服务器失败，出现异常");
                return false;
            }
            
        }
    }
}
