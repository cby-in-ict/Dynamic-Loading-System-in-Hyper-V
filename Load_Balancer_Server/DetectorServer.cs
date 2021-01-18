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

namespace Load_Balancer_Server
{
    public class DetectorServer
    {
        public string DetectorServerAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";\
        public DetectorServer(string Addr)
        {
            DetectorServerAddr = Addr;
        }

        [ServiceContract]
        public interface IServer
        {
            [OperationContract]
            bool TransferPerfAnalysis(Perf_Analysis perf_Detector);
            Perf_Transfer TransferPerfStr(string perf_Str);
        }

        class SampleServer : IServer
        {
            public bool TransferPerfAnalysis(Perf_Analysis perf_Detector)
            {
                Console.WriteLine($"Received {perf_Detector.CPU_K}");
                return true;
            }
            public Perf_Transfer TransferPerfStr(string perf_Str)
            {
                Console.WriteLine($"Received {perf_Str}");
#if Debug
                Console.WriteLine("收到性能特征字符串");
#endif
                Perf_Transfer perf_Transfer;
                byte[] buffer = Convert.FromBase64String(perf_Str);
                MemoryStream stream = new MemoryStream(buffer);
                IFormatter formatter = new BinaryFormatter();
                perf_Transfer = (Perf_Transfer)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();

                return perf_Transfer;
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
