using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TORCS_Bridge.TorcsIntegration
{
    class RunHeadless
    {
        //Command to start Torcs headless - .\wtorcs.exe -r config\raceman\mmcustom.xml

        //Results are stored in AppData\Local\torcs\results\mmcustom

        //Returns path to file
        public static string RunTorcs(string TORCSInstallPath, string TORCSResultsPath, int NumberOfRuns = 1, string RaceConfig = @"config\raceman\mmcustom.xml")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = "wtorcs.exe";
            startInfo.WorkingDirectory = TORCSInstallPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "-r " + RaceConfig;

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
            }

            var rawentries = Directory.GetFiles(TORCSResultsPath, "*.xml");

            return rawentries.OrderBy(t => t).Last();
        }
    }
}
