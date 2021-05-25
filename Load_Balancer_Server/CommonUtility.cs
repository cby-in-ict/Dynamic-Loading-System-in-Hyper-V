using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;

namespace Load_Balancer_Server
{
    public static class CommonUtility
    {
        public static string ExcelPath = @"C:\Users\CBY\Documents\GitHub\Dynamic-Loading-System-in-Hyper-V\Load_Balancer_Server\bin\VMMemStatistic.xlsx";
        public static ulong time = 0;
        public static ReaderWriterLockSlim toolLock = new ReaderWriterLockSlim();
        public static Collection<PSObject> RunPSCommand(string cmd)
        {
            using (Runspace runspace = RunspaceFactory.CreateRunspace())
            {
                bool result = false;
                runspace.Open();

                PowerShell ps = PowerShell.Create();

                ps.Runspace = runspace;

                ps.AddScript(cmd);

                //ps.Invoke();

                //ps.AddScript("$?");
                Collection<PSObject> output = ps.Invoke();
                //if (output != null)
                //{
                //    foreach (PSObject pSObject in output)
                //    {
                //        //Console.WriteLine(pSObject.ToString());
                //        result = Convert.ToBoolean(pSObject.ToString().ToLower());
                //    }
                //}

                runspace.Close();
                // fail, return false
                //if (result == false)
                //{
                //    return null;
                //}
                return output;
            }
        }

        // 间隔一段时间统计三个虚拟机的内存大小，并输出到Excel中
        public static void GetVMMemoryUsage(float detectTimeGap)
        {
            if (!File.Exists(ExcelPath))
            {
                Console.WriteLine("找不到输出的Excel文件，无法输出内存统计文件！");
                return;
            }
            System.Timers.Timer RUtimer = new System.Timers.Timer(detectTimeGap);    // 参数单位为ms
            RUtimer.Elapsed += DetectMemory;
            // 为true时，定时时间到会重新计时；为false则只定时一次
            RUtimer.AutoReset = true;
            // 使能定时器
            RUtimer.Enabled = true;
            // 开始计时
            RUtimer.Start();
        }
        public static void DetectMemory(object sender, ElapsedEventArgs e)
        {
            try
            {
                toolLock.EnterUpgradeableReadLock();
                toolLock.EnterWriteLock();
                ulong column = time / 5 + 2;
                //创建
                Excel.Application xlApp = new Excel.Application();
                xlApp.DisplayAlerts = false;
                xlApp.Visible = false;
                xlApp.ScreenUpdating = false;
                //打开Excel
                Excel.Workbook xlsWorkBook = xlApp.Workbooks.Open(ExcelPath, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
                if (VMState.VM1 != null & VMState.VM1.IsPowerOn())
                {
                    ulong VM1MemSize = VMState.VM1.GetPerformanceSetting().RAM_VirtualQuantity;
                    //处理数据
                    Excel.Worksheet sheet = xlsWorkBook.Worksheets[1];//工作薄从1开始，不是0
                    sheet.Cells[column, 1] = time.ToString();
                    sheet.Cells[column, 2] = VM1MemSize;
                }
                if (VMState.VM2 != null & VMState.VM2.IsPowerOn())
                {
                    ulong VM2MemSize = VMState.VM2.GetPerformanceSetting().RAM_VirtualQuantity;
                    Excel.Worksheet sheet = xlsWorkBook.Worksheets[1];//工作薄从1开始，不是0
                    sheet.Cells[column, 1] = time.ToString();
                    sheet.Cells[column, 3] = VM2MemSize;
                }
                if (VMState.VM3 != null & VMState.VM3.IsPowerOn())
                {
                    ulong VM3MemSize = VMState.VM3.GetPerformanceSetting().RAM_VirtualQuantity;
                    //处理数据
                    Excel.Worksheet sheet = xlsWorkBook.Worksheets[1];//工作薄从1开始，不是0
                    sheet.Cells[column, 1] = time.ToString();
                    sheet.Cells[column, 4] = VM3MemSize;
                }
                // 时间＋5s
                time += 5;
                xlsWorkBook.Save();
                xlApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                xlApp = null;
                return;
            }
            catch
            {
                return;
            }
            finally 
            {
                toolLock.ExitWriteLock();
                toolLock.ExitUpgradeableReadLock();
            }
        }
    }
}
