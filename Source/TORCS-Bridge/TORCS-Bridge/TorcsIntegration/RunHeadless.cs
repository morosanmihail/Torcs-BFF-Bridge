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

        //Returns path to results file
        public static string RunTorcs(string TORCSInstallPath, string TORCSResultsPath, int InstanceNumber, int NumberOfRuns = 1, string RaceConfig = @"config\raceman\mmcustom")
        {
            //TODO: move RaceConfig to RaceConfig+InstanceNumber
            File.Copy(Path.Combine(TORCSInstallPath, RaceConfig + ".xml"), Path.Combine(TORCSInstallPath, RaceConfig + InstanceNumber + ".xml"), true);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Path.Combine(TORCSInstallPath,"wtorcs.exe");
            startInfo.WorkingDirectory = TORCSInstallPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "-r " + RaceConfig + InstanceNumber + ".xml";

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

            var ConfigName = RaceConfig.Split('\\').Last() + InstanceNumber;

            var rawentries = Directory.GetFiles(Path.Combine(TORCSResultsPath, ConfigName), "*.xml");

            //Delete all except the last one
            for(int i=0;i<rawentries.Count() -1;i++)
            {
                File.Delete(rawentries[i]);
            }

            return rawentries.OrderBy(t => t).Last();
        }
    }
}
