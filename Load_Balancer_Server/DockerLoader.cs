using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Load_Balancer_Server
{
    public class DockerLoader
    {
        public class ContainerPerfInfo 
        {
            public string id { set; get; }
            public string name { set; get; }
            public double cpuPercentage { set; get; }
            public double memUsage { set; get; }
            public double memLimit { set; get; }
            public double memPercentage { set; get; }
            public int PIDS { set; get; }

        }
        DockerClient client = new DockerClientConfiguration(
            new Uri("npipe://./pipe/docker_engine"))
             .CreateClient();
        public async Task<IList<ContainerListResponse>> GetContainerListAsync()
        {
            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    Limit = 10,
                });
            foreach(ContainerListResponse container in containers)
            {
            }
            return containers;
        }

        public Stream GetContainerStats(string ContainerID)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
           //  Stream stats = client.Containers.GetContainerStatsAsync(ContainerID, new ContainerStatsParameters(), cancellation.Token);
            return stats;
        }

        public void GetContainerPerfInfo(string id = null, string name = null)
        {
            
        }

        public List<ContainerPerfInfo> GetContainerPerfInfoList()
        {
            List<ContainerPerfInfo> containerPerfInfos = new List<ContainerPerfInfo>();
            /* run cmd example:
             * PS C:\windows\system32> docker stats --no-stream
                CONTAINER ID   NAME               CPU %     MEM USAGE / LIMIT     MEM %     NET I/O          BLOCK I/O    PIDS
                9d450203744e   pensive_jennings   0.00%     504KiB / 2.924GiB     0.02%     586B / 0B        0B / 0B      1
             */
            Collection<PSObject> psResults = CommonUtility.RunPSCommand("docker stats --no-stream");
            if (psResults.Count == 0)
            {
                return null;
            }
            foreach (PSObject psResult in psResults)
            {
                ContainerPerfInfo currenteContainerPerfInfo = new ContainerPerfInfo();
                currenteContainerPerfInfo.id = Convert.ToString(psResult.Members["CONTAINER ID"].Value);
                currenteContainerPerfInfo.name = Convert.ToString(psResult.Members["NAME"].Value);
                currenteContainerPerfInfo.cpuPercentage = Convert.ToDouble(psResult.Members["CPU %"].Value.ToString().Replace("%", ""));
                currenteContainerPerfInfo.memPercentage = Convert.ToDouble(psResult.Members["MEM %"].Value.ToString().Replace("%", ""));
                string memInfo = Convert.ToString(psResult.Members["MEM USAGE / LIMIT"].Value);
                string memUsed = memInfo.Split(new char[3] { ' ', '/', ' ' })[0];
                string memLimit = memInfo.Split(new char[3] { ' ', '/', ' ' })[1];
                // extract memUsed, KB
                if (memUsed.Contains("KiB"))
                {
                    memUsed = memUsed.Replace("KiB", "");
                    currenteContainerPerfInfo.memUsage = Convert.ToDouble(memUsed);
                }
                else if (memUsed.Contains("MiB"))
                {
                    memUsed = memUsed.Replace("MiB", "");
                    currenteContainerPerfInfo.memUsage = Convert.ToDouble(memUsed) * 1024;
                }
                else if (memUsed.Contains("GiB"))
                {
                    memUsed = memUsed.Replace("GiB", "");
                    currenteContainerPerfInfo.memUsage = Convert.ToDouble(memUsed) * 1024 * 1024;
                }
                else 
                {
                    currenteContainerPerfInfo.memUsage = -1;
                }
                // extract memLimit, KB
                if (memLimit.Contains("KiB"))
                {
                    memLimit = memLimit.Replace("KiB", "");
                    currenteContainerPerfInfo.memLimit = Convert.ToDouble(memLimit);
                }
                else if (memLimit.Contains("MiB"))
                {
                    memLimit = memLimit.Replace("MiB", "");
                    currenteContainerPerfInfo.memLimit = Convert.ToDouble(memLimit) * 1024;
                }
                else if (memLimit.Contains("GiB"))
                {
                    memLimit = memLimit.Replace("GiB", "");
                    currenteContainerPerfInfo.memLimit = Convert.ToDouble(memLimit) * 1024 * 1024;
                }
                else
                {
                    currenteContainerPerfInfo.memLimit = -1;
                }
                currenteContainerPerfInfo.PIDS = Convert.ToInt32(psResult.Members["PIDS"].Value);
            }
            return containerPerfInfos;
        }
    }
}
