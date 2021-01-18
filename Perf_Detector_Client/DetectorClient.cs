using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perf_Detector_Client
{

    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        bool TransferPerfAnalysis(Perf_Transfer perf_Transfer);
        Perf_Transfer TransferPerfStr(string perf_Str);

    }

    public class DetectorClient : ClientBase<IServer>, IServer
    {
        public DetectorClient(EndpointAddress addy)
            : base(new HyperVNetBinding(), addy)
        {
        }

        public bool TransferPerfAnalysis(Perf_Transfer perf_Transfer) => Channel.TransferPerfAnalysis(perf_Transfer);

        public Perf_Transfer TransferPerfStr(string perf_Str) => Channel.TransferPerfStr(perf_Str);

        public void StartUpClient(string HvAddr)
        {
            Thread.Sleep(250);

            // Init Perf_Analysis class for detect performance
            Perf_Analysis perf_Detector = new Perf_Analysis();
            //string HvAddr = "hypervnb://0000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            HvAddr = "hypervnb://0000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";

            var client = new DetectorClient(new EndpointAddress(HvAddr));
            client.Open();

            //定时发送
            var d = client.TransferPerfStr("");
            //Console.WriteLine(d.Length);
            client.Close();
            Console.ReadLine();
        }
    }
}
