using System;
using System.Collections.Generic;
using System.Text;

namespace Perf_Detector
{
    public class CpuSpoofer
    {
        public static int CPUCount = 0;
        public static float CPUUsage = 0;
        public static int ProcessorQueueLength = 0;

        CpuSpoofer()
        {
            
        }
        public static bool RefreshCPUArg()
        {
            try
            {

                return true;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message, "刷新CPU参数异常！");
                return false;
            }
        }
    }
}
