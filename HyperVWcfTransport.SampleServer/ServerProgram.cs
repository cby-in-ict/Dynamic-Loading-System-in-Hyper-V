using HyperVWcfTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace HyperVWcfTransport.SampleServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
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

    class Program
    {
        static void Main(string[] args)
        {
            var sh = new ServiceHost(new SampleServer());
            var binding = new HyperVNetBinding();
            // https://docs.microsoft.com/zh-cn/dotnet/api/system.servicemodel.servicehost.-ctor?view=netframework-4.8#System_ServiceModel_ServiceHost__ctor_System_Object_System_Uri___
            sh.AddServiceEndpoint(typeof(IServer), binding, "hypervnb://00000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47");
            sh.Open();
            Console.ReadLine();
            sh.Close();
        }
    }
}
