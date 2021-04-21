using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Microsoft.Windows.ComputeVirtualization;

namespace Load_Balancer_Server
{
    public class HvContainer
    {
        public IList<Container> HvContainerList { set; get; }
        public HvContainer()
        {
            HcsFactory hcsFactory = new HcsFactory();
            IHcs Hvhcs = HcsFactory.GetHcs();
            DockerMonitor dockerLoader = new DockerMonitor();
            IList<ContainerListResponse> containerListResponses = dockerLoader.GetContainerListAsync().GetAwaiter().GetResult();
            foreach (ContainerListResponse c in containerListResponses)
            {
                Container container = HostComputeService.GetComputeSystem(c.ID);
                dockerLoader.GetContainerStats(c.ID).GetAwaiter().GetResult();
                //Console.WriteLine(perfInfo.memLimit);
                //Console.WriteLine(container.GetType().Name);

                //ContainerSettings containerSettings = new ContainerSettings();

                //Console.WriteLine(container.);
            }
            
        }
        

    }
}
