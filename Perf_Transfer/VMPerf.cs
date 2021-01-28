using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf_Transfer
{
    [Serializable]
    public class VMPerf
    {
        // add src VMName
        public string VMName { get; set; }
        public int CPUCount = 0;
        public float ProcessorQueueLength { get; set; }
        public float CPUProcessorTime { get; set; }
        public float CPUPrivilegedTime { get; set; }
        public float CPUInterruptTime { get; set; }
        public float CPUDPCTime { get; set; }
        public float MEMAvailable { get; set; }
        public float MEMCommited { get; set; }
        public float MEMCommitLimit { get; set; }
        public float MEMCommitedPerc { get; set; }
        public float MEMPoolPaged { get; set; }
        public float MEMPoolNonPaged { get; set; }
        public float MEMCached { get; set; }
        public float PageFile { get; set; }
        public float PagesPerSec { get; set; }
        public float PageFaultsPerSec { get; set; }
        public float PageReadsPerSec { get; set; }
        public float PageWritesPerSec { get; set; }
        public float PagesInputPerSec { get; set; }
        public float PagesOutputPerSec { get; set; }
    }
}
