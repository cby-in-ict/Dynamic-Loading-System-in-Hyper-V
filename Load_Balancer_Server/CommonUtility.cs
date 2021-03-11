using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace Load_Balancer_Server
{
    public static class CommonUtility
    {
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
    }
}
