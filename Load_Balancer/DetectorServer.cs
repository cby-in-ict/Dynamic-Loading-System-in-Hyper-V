using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace HyperVWcfTransport
{
    public class DetectorServer
    {
        public interface IServer
        {
            [OperationContract]
            byte[] DoThing(string foo);
        }
        class SampleServer : IServer
        {
            public byte[] DoThing(string foo)
            {
                Console.WriteLine($"Received {foo}");
                var d = new byte[64 * 1024 * 1024];
                var rand = new System.Security.Cryptography.RNGCryptoServiceProvider();
                rand.GetBytes(d);
                return d;
            }
        }
    }
}
