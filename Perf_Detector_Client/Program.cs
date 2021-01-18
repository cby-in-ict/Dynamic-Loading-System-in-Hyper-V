using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf_Detector_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string HvAddr = "hypervnb://0000000-0000-0000-0000-000000000000/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            // 00000000-0000-0000-0000-000000000000
            Client client = new Client(HvAddr);
            client.StartUpClient();
        }
    }
}
