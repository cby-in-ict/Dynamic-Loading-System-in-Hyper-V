using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Perf_Detector
{
    public class DetectorServer
    {
        public string DetectorServerAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";

        [ServiceContract]
        public interface IServer
        {
            [OperationContract]
            byte[] TransferPerfAnalysis(Perf_Analysis perf_Detector);
        }

        class SampleServer : IServer
        {
            public byte[] TransferPerfAnalysis(Perf_Analysis perf_Detector)
            {
                Console.WriteLine($"Received {perf_Detector}");
                var d = new byte[64 * 1024 * 1024];
                var rand = new System.Security.Cryptography.RNGCryptoServiceProvider();
                rand.GetBytes(d);
                return d;
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
