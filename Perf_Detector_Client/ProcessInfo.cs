using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Perf_Detector_Client
{
    public class ProcessInfo
    {
        public List<string> TaskList = new List<string>();
        public Dictionary<string, float> ProcMemDict = new Dictionary<string, float>();
        public string TaskListCmd = "tasklist /V /FO \"CSV\"";

        public string RedirectCmd = @" > D:\processInfo.csv";
        public string RedirectTaskListCmd = "tasklist /V /FO \"CSV\" > D:\\processInfo.csv";
        public List<string> TopFiveMemTask = new List<string>();

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
    }
}
