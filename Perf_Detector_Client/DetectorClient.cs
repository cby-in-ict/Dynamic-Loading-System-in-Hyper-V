using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perf_Detector
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(250);

            // Init Perf_Analysis class for detect performance
            Perf_Analysis perf_Detector = new Perf_Analysis();

            var client = new DetectorClient(new EndpointAddress("hypervnb://e0e16197-dd56-4a10-9195-5ee7a155a838/C7240163-6E2B-4466-9E41-FF74E7F0DE47"));
            client.Open();
            var d = client.TransferPerfAnalysis(perf_Detector);
            Console.WriteLine(d.Length);
            client.Close();
            Console.ReadLine();
        }
    }

    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        byte[] TransferPerfAnalysis(Perf_Analysis perf_Detector);

    }

    class DetectorClient : ClientBase<IServer>, IServer
    {
        public DetectorClient(EndpointAddress addy)
            : base(new HyperVNetBinding(), addy)
        {
        }

        public byte[] TransferPerfAnalysis(Perf_Analysis perf_Detector) => Channel.TransferPerfAnalysis(perf_Detector);
    }
}
