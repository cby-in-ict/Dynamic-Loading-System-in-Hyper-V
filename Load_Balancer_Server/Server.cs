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
using System.Threading;

namespace Load_Balancer_Server
{
    public class Server
    {
        public string DetectorServerAddr = "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
        public static DetectorServer mySampleServer;
       // public static VMPerf currentPerfTransfer;
        public Server(string Addr)
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
        public class DetectorServer : IServer
        {
            // 字典类型，记录各虚拟机实时性能
            public Dictionary<string, VMPerf> vmPerfDict = new Dictionary<string, VMPerf>();
            public VMPerf TransferPerfStr(string perf_Str)
            {
                try
                {
#if Debug
                Console.WriteLine("收到性能特征字符串");
#endif
                    // 反序列化
                    VMPerf perf_Transfer;
                    byte[] buffer = Convert.FromBase64String(perf_Str);
                    if (buffer.Length < 5)
                    { 
                        return null;
                    }
                    MemoryStream stream = new MemoryStream(buffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    perf_Transfer = (VMPerf)formatter.Deserialize(stream);
                    stream.Flush();
                    stream.Close();
                    Monitor.Enter(this);
                    // TODO: 回调函数，处理perf_Transfer
                    if (vmPerfDict.ContainsKey(perf_Transfer.VMName))
                    {
                        vmPerfDict[perf_Transfer.VMName] = perf_Transfer;
                    }
                    else
                    {
                        vmPerfDict.Add(perf_Transfer.VMName, perf_Transfer);
                    }
                    Monitor.Exit(this);
#if Debug
                Console.WriteLine("收到虚拟机：" + perf_Transfer.VMName + "的消息，" + "\nCPU占用率为：" + Convert.ToString(vmPerfDict[perf_Transfer.VMName].CPUPrivilegedTime));
#endif
                    return perf_Transfer;
                }
                catch (Exception exp)
                {
                    Console.WriteLine("接收消息异常：" + exp.Message);
                    return null;
                }

            }

        }
        public void StartUpServer()
        {
            try
            {
                mySampleServer = new DetectorServer();
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
