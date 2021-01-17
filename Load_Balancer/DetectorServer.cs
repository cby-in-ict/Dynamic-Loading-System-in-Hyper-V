//using HyperVWcfTransport;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.Text;
//using System.Threading.Tasks;

//namespace HyperVWcfTransport
//{
//    public class DetectorServer
//    {
//        public string DetectorServerAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
//        public interface IServer
//        {
//            public byte[] DoThing(string foo)
//            {
//                Console.WriteLine($"Received {foo}");
//                var d = new byte[64 * 1024 * 1024];
//                var rand = new System.Security.Cryptography.RNGCryptoServiceProvider();
//                rand.GetBytes(d);
//                return d;
//            }
//        }
//        class SampleServer : IServer
//        {
//            public byte[] DoThing(string foo)
//            {
//                Console.WriteLine($"Received {foo}");
//                var d = new byte[64 * 1024 * 1024];
//                var rand = new System.Security.Cryptography.RNGCryptoServiceProvider();
//                rand.GetBytes(d);
//                return d;
//            }
//        }
//        public bool StartUpServer()
//        {
//            try
//            {   
//                var sh = new ServiceHost(new SampleServer());
//                var binding = new HyperVNetBinding();
//                sh.AddServiceEndpoint(typeof(IServer), binding, DetectorServerAddr);
//                sh.Open();
//                Console.ReadLine();
//                sh.Close();
//                return true;
//            }
//            catch (Exception exp)
//            {
//                Console.WriteLine(exp.Message, "启动服务器失败，出现异常");
//                return false;
//            }
            
//        }
//    }
//}
