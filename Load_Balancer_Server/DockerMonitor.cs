﻿using System;
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

    public class PerfData
    {
        public DateTime Timestamp { get; set; }
        public ulong? TotalCpuUsage { get; set; }
        public ulong? TotalMemoryUsage { get; set; }
        public ulong? MemoryLimit { get; set; }
        public ulong? ReadBytes { get; set; }
        public ulong? WriteBytes { get; set; }
        public string ContainerName { get; set; }
        public string ContainerId { get; set; }
    }
    public class DockerMonitor
    {
        public DockerMonitor() 
        {
            GetContainerListAsync();
        }
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

       

        public Dictionary<string, PerfData> PerfDictByName = new Dictionary<string, PerfData>();

        public Dictionary<string, PerfData> PerfDictByID = new Dictionary<string, PerfData>();

        DockerClient client = new DockerClientConfiguration(
            new Uri("npipe://./pipe/docker_engine"))
             .CreateClient();
        public IList<ContainerListResponse> containerListResponses;
        public async Task<List<ContainerListResponse>> GetContainerListAsync()
        {
            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    Limit = 10,
                });
            containerListResponses = containers.ToList();
            return containers.ToList();
        }
        public class StatsProgress : IProgress<ContainerStatsResponse>
        {
            public void Report(ContainerStatsResponse value)
            {
                Console.WriteLine(value.ToString());
            }
        }
        
        public async Task<StatsProgress> GetContainerStatsAsync(string ContainerID, ContainerStatsParameters containerStatsParameters)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            IProgress<ContainerStatsResponse> progress1 = null;
            StatsProgress sp = new StatsProgress();
            client.Containers.GetContainerStatsAsync(ContainerID, new ContainerStatsParameters
            {
                Stream = false
            },
            progress1, default).Wait();
            Console.WriteLine(Convert.ToString(progress1));

            Stream stream = await client.Containers.GetContainerStatsAsync(ContainerID, new ContainerStatsParameters
            {
                Stream = false
            },
            default);

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            Console.WriteLine(text);
            Console.WriteLine(Convert.ToString(progress1));

            return sp;
        }

        /// <summary>
        /// Get the performance stats for a container
        /// </summary>
        /// <param name="client">Docker environment instance</param>
        /// <param name="containerID">Name of the container</param>
        /// <returns></returns>
        public async Task GetContainerStats(string containerID)
        {
            //containerID = containerID.Trim('/');
            Console.WriteLine("Getting performance data for containerID {0}:", containerID);
            //Setting to stream to false returns only one data point. Setting this to yes returns data at continuous intervals.
            var statsParameters = new ContainerStatsParameters { Stream = true };

            IProgress<ContainerStatsResponse> progress =
                new Progress<ContainerStatsResponse>(GetPerformanceStats);

            await client.Containers.GetContainerStatsAsync(containerID, statsParameters, progress, CancellationToken.None);
        }

        private void GetPerformanceStats(ContainerStatsResponse stats)
        {
            if (stats == null) return;

            var perfData = new PerfData
            {
                TotalCpuUsage = stats.CPUStats?.CPUUsage?.TotalUsage,
                TotalMemoryUsage = stats.MemoryStats?.Usage,
                MemoryLimit = stats.MemoryStats ?.Limit,
                ReadBytes = stats.StorageStats?.ReadSizeBytes,
                WriteBytes = stats.StorageStats?.WriteSizeBytes,
                Timestamp = stats.Read,
                ContainerName = stats.Name.Trim('/'),
                ContainerId = stats.ID
            };
            Console.WriteLine("Writing data to PerfDictByName");
            //_elasticClient.WriteToEs(perfData);
            if (PerfDictByName.ContainsKey(perfData.ContainerName))
            {
                PerfDictByName[perfData.ContainerName] = perfData;
            }
            else
            {
                PerfDictByName.Add(perfData.ContainerName, perfData);
            }

            if (PerfDictByID.ContainsKey(perfData.ContainerId))
            {
                PerfDictByID[perfData.ContainerId] = perfData;
            }
            else
            {
                PerfDictByID.Add(perfData.ContainerId, perfData);
            }
        }

        public async void test()
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            //Stream testStream = await client.Containers.GetContainerStatsAsync()
            //Stream stream = await client.Miscellaneous.MonitorEventsAsync(new ContainerEventsParameters(), cancellation.Token);
        }
        public async Task<ContainerUpdateResponse> UpdateContainer(string id, ContainerUpdateParameters updateParameters)
        {
            ContainerUpdateResponse updateResponse = await client.Containers.UpdateContainerAsync(id, updateParameters);
            return updateResponse;
        }

        
        public ContainerPerfInfo GetContainerPerfInfo(string id = null, string name = null)
        {
            if (id != null)
            {
                List<ContainerPerfInfo> containerPerfInfos = GetContainerPerfInfoListByPS();
                ContainerPerfInfo foundContainerInfo = containerPerfInfos.Find(c => c.id == id);
                return foundContainerInfo;
            }
            else if (name != null)
            {
                List<ContainerPerfInfo> containerPerfInfos = GetContainerPerfInfoListByPS();
                ContainerPerfInfo foundContainerInfo = containerPerfInfos.Find(c => c.id == id);
                return foundContainerInfo;
            }
            else 
            {
                return null;
            }
        }

        public List<ContainerPerfInfo> GetContainerPerfInfoListByPS()
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
            bool isFirst = true;
            foreach (PSObject psResult in psResults)
            {
                //if (isFirst)
                //{
                //    isFirst = false;
                //    continue;
                //}
                foreach (PSPropertyInfo prop in psResult.Properties)
                {
                    var count = prop.Name;
                    var name = prop.Value;
                    //In other words generate output as you desire.
                    Console.WriteLine("name is:" + count.ToString());
                    Console.WriteLine("value is:" + name.ToString() );
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(psResult.ToString());

                string perfStr = stringBuilder.ToString();
                Console.WriteLine(perfStr);
                string [] perfItems = perfStr.Split('\t');
                foreach (string perfItem in perfItems)
                {
                    Console.WriteLine("\n" + perfItem);
                }

                ContainerPerfInfo currenteContainerPerfInfo = new ContainerPerfInfo();
                Console.WriteLine(Convert.ToString(psResult));
                currenteContainerPerfInfo.id = Convert.ToString(psResult.Properties["CONTAINER ID"].Value);
                currenteContainerPerfInfo.name = Convert.ToString(psResult.Properties["NAME"].Value);
                currenteContainerPerfInfo.cpuPercentage = Convert.ToDouble(psResult.Properties["CPU %"].Value.ToString().Replace("%", ""));
                currenteContainerPerfInfo.memPercentage = Convert.ToDouble(psResult.Properties["MEM %"].Value.ToString().Replace("%", ""));
                string memInfo = Convert.ToString(psResult.Properties["MEM USAGE / LIMIT"].Value);
                string memUsed = memInfo.Split(new char[3] { ' ', '/', ' ' })[0];
                string memLimit = memInfo.Split(new char[3] { ' ', '/', ' ' })[1];
                /*currenteContainerPerfInfo.id = Convert.ToString(psResult.Members["CONTAINER ID"].Value);
                currenteContainerPerfInfo.id = Convert.ToString(psResult.Members["CONTAINER ID"].Value);
                currenteContainerPerfInfo.name = Convert.ToString(psResult.Members["NAME"].Value);
                currenteContainerPerfInfo.cpuPercentage = Convert.ToDouble(psResult.Members["CPU %"].Value.ToString().Replace("%", ""));
                currenteContainerPerfInfo.memPercentage = Convert.ToDouble(psResult.Members["MEM %"].Value.ToString().Replace("%", ""));
                string memInfo = Convert.ToString(psResult.Members["MEM USAGE / LIMIT"].Value);
                string memUsed = memInfo.Split(new char[3] { ' ', '/', ' ' })[0];
                string memLimit = memInfo.Split(new char[3] { ' ', '/', ' ' })[1];*/
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