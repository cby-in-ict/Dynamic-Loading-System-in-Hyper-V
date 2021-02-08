using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Data;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Perf_Detector_Client
{
    public class ProcessInfo
    {
        public int procID { get; set; }
        public string procName { get; set; }
        public long memSize { get; set; }

        public ProcessInfo(int id, string name, long memorySize)
        {
            procID = id;
            procName = name;
            memSize = memorySize;
        }
    }

    public class ProcessControler
    {
        public List<string> TaskList = new List<string>();
        public Dictionary<string, float> ProcMemDict = new Dictionary<string, float>();
        public string TaskListCmd = "tasklist /V /FO \"CSV\"";

        public string RedirectCmd = @" > D:\processInfo.csv";
        public string RedirectTaskListCmd = "tasklist /V /FO \"CSV\" > D:\\TEMP\\processInfo1.csv";
        public List<string> TopFiveMemTask = new List<string>();

        public Dictionary<string, Int64> BlackProcDict = new Dictionary<string, long>();
        Dictionary<string, Int64> WhiteProcDict = new Dictionary<string, long>();
        private System.Timers.Timer RUtimer;
        public string ProcConfigPath { set; get; }
        public int CheckingTimeSpan = 10000;
        public ProcessControler(string procConfigPath, int checkingTimeSpan)
        {
            ProcConfigPath = procConfigPath;
            CheckingTimeSpan = checkingTimeSpan;
        }
        public List<ProcessInfo> GetProcInfoList()
        {
            List<ProcessInfo> procInfoList = new List<ProcessInfo>();
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                try
                {
                    procInfoList.Add(new ProcessInfo(p.Id, p.ProcessName, p.VirtualMemorySize64));
                }
                catch (Exception e)
                {
                    Console.WriteLine("收集进程信息异常:" + e.Message);
                }
            }
            return procInfoList;
        }
        public bool GetBlackListFromConfig()
        {
            try
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(ProcConfigPath))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);

                        if (o.ContainsKey("BlackList"))
                        {
                            JArray ja = ((JArray)o["BlackList"]);
                            for (int i = 0; i < ja.Count; i++)
                            {
                                string procName = ((JObject)(ja.ElementAt(i)))["name"].ToString();
                                long memLimit = Convert.ToInt64(((JObject)(ja.ElementAt(i)))["memLimit"].ToString());
                                BlackProcDict.Add(procName, memLimit);
                            }
                        }
                        else
                        {
                            Console.WriteLine("黑名单为空");
                        }
                        return true;
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("读取黑名单进程异常，异常为：" + exp.Message);
                return false;
            }

        }
        public bool GetWhiteListFromConfig()
        {
            try
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(ProcConfigPath))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);

                        if (o.ContainsKey("WhiteList"))
                        {
                            JArray ja = ((JArray)o["WhiteList"]);
                            for (int i = 0; i < ja.Count; i++)
                            {
                                string procName = ((JObject)(ja.ElementAt(i)))["name"].ToString();
                                long memLimit = Convert.ToInt64(((JObject)(ja.ElementAt(i)))["memLimit"].ToString());
                                WhiteProcDict.Add(procName, memLimit);
                            }
                        }
                        else
                        {
                            Console.WriteLine("白名单为空");
                        }
                        return true;
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("读取黑名单进程异常，异常为：" + exp.Message);
                return false;
            }
        }

        public void TryKillBlackProcs(List<ProcessInfo> procList)
        {
            foreach (ProcessInfo proc in procList)
            {
                if (proc.procName == null)
                    continue;
                if (BlackProcDict.ContainsKey(proc.procName))
                {
                    if (BlackProcDict[proc.procName] == 0)
                    {
                        Process[] p = Process.GetProcessesByName(proc.procName);
                        foreach (Process procToKill in p)
                        {
                            try
                            {
                                procToKill.Kill();
                                Console.WriteLine("杀死进程：" + procToKill.ProcessName + "成功，其PID为：" + Convert.ToString(procToKill.Id));
                            }
                            catch (Exception exp)
                            {
                                //Console.WriteLine("杀死进程：" + procToKill.ProcessName + " 失败，异常为:" + exp.Message + "\n可尝试管理员权限启动本程序");
                            }
                        }
                    }
                    else if (BlackProcDict[proc.procName] * 1024 * 1024 < proc.memSize)
                    {
                        Process[] p = Process.GetProcessesByName(proc.procName);
                        foreach (Process procToKill in p)
                        {
                            try
                            {
                                procToKill.Kill();
                                Console.WriteLine("杀死进程：" + procToKill.ProcessName + "成功，其PID为：" + Convert.ToString(procToKill.Id));
                            }
                            catch (Exception exp)
                            {
                                //Console.WriteLine("杀死进程：" + procToKill.ProcessName + " 失败，异常为:" + exp.Message + "\n可尝试管理员权限启动本程序");
                            }
                        }
                    }
                }
            }
        }

        public void CheckProcessByTime()
        {
            while (!File.Exists(ProcConfigPath))
            {
                Thread.Sleep(10000);
            }
            bool ret = GetBlackListFromConfig();
            if (!ret)
            {
                Console.WriteLine("读取配置文件：" + ProcConfigPath +" 失败，请检查json格式！");
                return;
            }
            // 创建一个100ms定时的定时器
            RUtimer = new System.Timers.Timer(CheckingTimeSpan);    // 参数单位为ms
                                                                    // 定时时间到，处理函数为OnTimedUEvent(...)
            RUtimer.Elapsed += CheckProcess;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }
        public void CheckProcess(object sender, ElapsedEventArgs e)
        {
            List<ProcessInfo> processInfoList = GetProcInfoList();
            TryKillBlackProcs(processInfoList);
        }

        public void StartUpCheckingProcess()
        {

            var task = new Task(() =>
            {
                CheckProcessByTime();
            });
            task.Start();
        }

        // Unused funcs

        public string getTaskList()
        {
            RunCMDCommand(TaskListCmd, out string ret);
            return ret;
        }
        public string getRedirectTaskList()
        {
            RunCMDCommand(RedirectTaskListCmd, out string ret);
            return ret;
        }

        private static string CMDPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";
        public static void RunCMDCommand(string Command, out string OutPut)
        {
            try
            {
                using (Process pc = new Process())
                {
                    Command = Command.Trim().TrimEnd('&') + "&exit";

                    pc.StartInfo.FileName = CMDPath;
                    pc.StartInfo.CreateNoWindow = true;
                    pc.StartInfo.RedirectStandardError = true;
                    pc.StartInfo.RedirectStandardInput = true;
                    pc.StartInfo.RedirectStandardOutput = true;
                    pc.StartInfo.UseShellExecute = false;

                    pc.Start();

                    pc.StandardInput.WriteLine(Command);
                    pc.StandardInput.AutoFlush = true;

                    OutPut = pc.StandardOutput.ReadToEnd();
                    int P = OutPut.IndexOf(Command) + Command.Length;
                    if ((OutPut.Length - P - 3) > 0)
                        OutPut = OutPut.Substring(P, OutPut.Length - P - 3);
                    pc.WaitForExit();
                    pc.Close();
                    return;
                }
            }
            catch (Exception exp)
            {
                OutPut = exp.Message;
                Console.WriteLine(OutPut);
                return;
            }

        }

        public bool KillPorcByCmd(string processName)
        {
            string output = "";
            string killProcCmd = "taskkill /F /IM ";
            killProcCmd = killProcCmd + processName;
            RunCMDCommand(killProcCmd, out string ret);
            if (ret.Contains(""))
                return true;
            else
                return false;


        }
        public void KillProcess(string processName)
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName.Contains(processName))
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(); // possibly with a timeout
                        Console.WriteLine($"已杀掉{processName}进程！！！");
                    }
                    catch (Win32Exception e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }
                }

            }
        }
       
    }
}
