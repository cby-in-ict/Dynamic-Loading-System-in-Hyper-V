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
            string HvAddr = "hypervnb://a42e7cda-d03f-480c-9cc2-a4de20abb878/C7240163-6E2B-4466-9E41-FF74E7F0DE47";
            Client client = new Client(HvAddr);
            client.StartUpClient();
        }
    }
}
