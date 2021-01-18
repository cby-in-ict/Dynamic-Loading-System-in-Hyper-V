using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Perf_Transfer;

namespace Perf_Detector_Client
{

    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        bool TransferPerfAnalysis(VMPerf perf_Transfer);
        VMPerf TransferPerfStr(string perf_Str);

    }

    public class DetectorClient : ClientBase<IServer>, IServer
    {
        public DetectorClient(EndpointAddress addy)
            : base(new HyperVNetBinding(), addy)
        {
        }

        public bool TransferPerfAnalysis(VMPerf perf_Transfer) => Channel.TransferPerfAnalysis(perf_Transfer);

        public VMPerf TransferPerfStr(string perf_Str) => Channel.TransferPerfStr(perf_Str);

        
    }
    public class Client 
    {
        private System.Timers.Timer RUtimer;
        public string HvAddr { set; get; }
        public Perf_Analysis client_perf_Analysis;
        public DetectorClient detectorClient;
        public Client(string Addr)
        {
            client_perf_Analysis = new Perf_Analysis();
            HvAddr = Addr;
            
        }
        public void StartUpClient()
        {
            Thread.Sleep(250);

            // Init Perf_Analysis class for detect performance
            Perf_Analysis perf_Detector = new Perf_Analysis();
            //string HvAddr = "hypervnb://0000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";

            detectorClient = new DetectorClient(new EndpointAddress(HvAddr));
            detectorClient.Open();

            //定时发送
            SendByTime();
            //detectorClient.Close();
            Console.ReadLine();
        }

        public void SendByTime()
        {
            // 创建一个100ms定时的定时器
            RUtimer = new System.Timers.Timer(100);    // 参数单位为ms
                                                       // 定时时间到，处理函数为OnTimedUEvent(...)
            RUtimer.Elapsed += SendPerfTransfer;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }

        public void SendPerfTransfer(object sender, ElapsedEventArgs e)
        {
            try
            {
                client_perf_Analysis.SetPerfTransfer(client_perf_Analysis.memSpoofer, client_perf_Analysis.cpuSpoofer);
                // Serial perf_transfer
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, client_perf_Analysis.perf_Transfer);
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Close();
                string perfTransferStrSerial = Convert.ToBase64String(buffer);
                detectorClient.TransferPerfStr(perfTransferStrSerial);

                return;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "发送性能指标参数异常！");
                return;
            }
        }
    }
}
