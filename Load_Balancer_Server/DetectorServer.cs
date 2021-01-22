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
using System.ServiceModel.Channels;

namespace Load_Balancer_Server
{
    public class DetectorServer
    {
        public string DetectorServerAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
        public static SampleServer mySampleServer;
       // public static VMPerf currentPerfTransfer;
        public DetectorServer(string Addr)
        {
            DetectorServerAddr = Addr;
        }

        [ServiceContract]
        public interface IServer
        {
            [OperationContract]
            VMPerf TransferPerfStr(string perf_Str);

        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
        public class SampleServer : IServer
        {
            // 字典类型，记录各虚拟机实时性能
            public Dictionary<string, VMPerf> vmPerfDict = new Dictionary<string, VMPerf>();
            public VMPerf TransferPerfStr(string perf_Str)
            {
#if Debug
                Console.WriteLine("收到性能特征字符串");
#endif
                // 反序列化
                VMPerf perf_Transfer;
                byte[] buffer = Convert.FromBase64String(perf_Str);
                MemoryStream stream = new MemoryStream(buffer);
                BinaryFormatter formatter = new BinaryFormatter();
                perf_Transfer = (VMPerf)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();

                // TODO: 回调函数，处理perf_Transfer
                if (vmPerfDict.ContainsKey(perf_Transfer.VMName))
                {
                    vmPerfDict[perf_Transfer.VMName] = perf_Transfer;
                }
                else
                {
                    vmPerfDict.Add(perf_Transfer.VMName, perf_Transfer);
                }
#if Debug
                Console.WriteLine("收到虚拟机：" + perf_Transfer.VMName + "的消息，" + "\nCPU占用率为：" + Convert.ToString(vmPerfDict[perf_Transfer.VMName].CPUPrivilegedTime));
#endif              
                return perf_Transfer;
            }

        }
        public void StartUpServer()
        {
            try
            {
                mySampleServer = new SampleServer();
                var sh = new ServiceHost(mySampleServer);
                var binding = new HyperVNetBinding();
                sh.AddServiceEndpoint(typeof(IServer), binding, DetectorServerAddr);
                sh.Open();
                Console.ReadLine();
                sh.Close();
                //return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "启动服务器失败，出现异常");
                //return false;
            }
            
        }
    }
}
